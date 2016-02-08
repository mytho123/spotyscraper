using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
using SpotyScraper.Spotify.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SpotyScraper.Spotify
{
    [Export(typeof(IStreamService))]
    [ExportMetadata(nameof(IStreamServiceData.Name), NAME)]
    [ExportMetadata(nameof(IStreamServiceData.Description), DESCRIPTION)]
    internal class SpotifyService : IStreamService
    {
        public const string NAME = "Spotify";
        public const string DESCRIPTION = "Spotify streaming service";
        public readonly TimeSpan REQUESTSDELAY = TimeSpan.FromMilliseconds(25);

        public string Name { get; } = NAME;

        public string Description { get; } = DESCRIPTION;

        #region Authentication

        private const int PORT = 30968;
        private const string CLIENTID = "d49d7e00e3eb448784c74177a0254656";

        private readonly string _state = new Random().Next(int.MinValue, int.MaxValue).ToString();
        private readonly SemaphoreSlim _authenticationSemaphore = new SemaphoreSlim(0, 1);

        private SpotifyWebAPI _spotify;
        private ImplicitGrantAuth _authentication;

        private async Task<bool> CheckAuthentication()
        {
            if (Settings.Default.TokenExpirationDate > DateTime.Now)
            {
                InitSpotifyWebAPI();
            }
            else
            {
                this.StartAuthentication();
                await _authenticationSemaphore.WaitAsync();
            }

            return _spotify != null;
        }

        private void StartAuthentication()
        {
            _authentication = new ImplicitGrantAuth
            {
                RedirectUri = $"http://localhost:{PORT}",
                ClientId = CLIENTID,
                Scope = Scope.PlaylistReadPrivate | Scope.PlaylistModifyPrivate | Scope.PlaylistModifyPublic,
                State = _state,
            };
            _authentication.OnResponseReceivedEvent += _authentication_OnResponseReceivedEvent;

            _authentication.StartHttpServer(PORT);
            _authentication.DoAuth();
        }

        private void _authentication_OnResponseReceivedEvent(Token token, string state)
        {
            _authentication.StopHttpServer();

            try
            {
                if (state != _state)
                    throw new InvalidOperationException($"{nameof(SpotifyService)} - Wrong state received");

                if (token.Error != null)
                    throw new InvalidOperationException($"{nameof(SpotifyService)} - Error: {token.Error}");

                Settings.Default.Token = token.AccessToken;
                Settings.Default.TokenType = token.TokenType;
                Settings.Default.TokenExpirationDate = DateTime.Now + TimeSpan.FromSeconds(token.ExpiresIn);
                Settings.Default.Save();

                InitSpotifyWebAPI();
            }
            finally
            {
                _authenticationSemaphore.Release();
            }
        }

        private void InitSpotifyWebAPI()
        {
            _spotify = new SpotifyWebAPI
            {
                UseAuth = true,
                AccessToken = Settings.Default.Token,
                TokenType = Settings.Default.TokenType,
            };
        }

        #endregion Authentication

        public async Task ResolveAsync(IEnumerable<Track> tracks, IProgress<double> progress)
        {
            if (!await this.CheckAuthentication())
                return;

            var tracksFixed = tracks.ToArray();
            int nbDone = 0;

            foreach (var track in tracksFixed)
            {
                await this.ResolveAsync(track);
                progress.Report((double)++nbDone / (double)tracksFixed.Length);
                await Task.Delay(REQUESTSDELAY); // avoid "API rate limit exceeded"
            }
        }

        public async Task ResolveAsync(Track track)
        {
            string title;
            string artist;
            this.PrepareTrack(track, out title, out artist);

            var q = $"track:%22{title}%22&artist:\"{artist}\"";
            q = PreprocessRequest(q);
            var searchItem = await _spotify.SearchItemsAsync(q, SearchType.Track, 50);
            LogIfError(q, searchItem);

            // if no result found, search for title only
            if (searchItem.Tracks?.Items?.Count == 0)
            {
                await Task.Delay(REQUESTSDELAY); // avoid "API rate limit exceeded"

                q = $"track:%22{title}%22";
                q = PreprocessRequest(q);
                searchItem = await _spotify.SearchItemsAsync(q, SearchType.Track, 50);
                LogIfError(q, searchItem);
            }

            var spotifyTracks = searchItem?.Tracks?.Items
                .OrderByDescending(x => x.Popularity)
                .Select(x => new SpotifyTrack(x));

            if (spotifyTracks != null)
                track.SetMatches(spotifyTracks);
        }

        private static void LogIfError(string q, SearchItem searchItem)
        {
            if (searchItem.HasError())
            {
                Debug.WriteLine($"Spotify returned an error: {searchItem.Error}");
                Debug.WriteLine($"Request was: {q}");
            }
        }

        private void PrepareTrack(Track track, out string title, out string artist)
        {
            title = track.Title;
            title = RemoveFromCharacterToEnd(title, '(');
            title = RemoveFromCharacterToEnd(title, '[');
            //title = title.Replace('\'', ' ');

            artist = track.Artists.FirstOrDefault();
            if (artist.EndsWith("..."))
            {
                var lastSpaceIndex = artist.LastIndexOf(' ');
                if (lastSpaceIndex != -1)
                    artist = artist.Substring(0, lastSpaceIndex);
            }
            artist = artist
                .Replace('\'', ' ')
                .Replace('.', ' ');
        }

        private static string RemoveFromCharacterToEnd(string title, char character)
        {
            var parenthesisIndex = title.IndexOf(character);
            if (parenthesisIndex > 1)
            {
                title = title.Substring(0, parenthesisIndex).Trim();
            }
            return title;
        }

        private static string PreprocessRequest(string request)
        {
            // spotify uses a kind of Uri.EscapeDataString(request)
            request = request
                .Replace("\"", "%22")
                .Replace("#", "%23")
                .Replace("&", "%26")
                .Replace("'", "%27")
                .Replace("+", "%2B");
            return request;
        }

        public async Task CreatePlaylist(string playlistName, IEnumerable<ITrackMatch> tracks)
        {
            if (!await this.CheckAuthentication())
                return;

            var user = await _spotify.GetPrivateProfileAsync();
            var playlist = await _spotify.CreatePlaylistAsync(user.Id, playlistName);
            if (playlist.HasError())
            {
                Debug.WriteLine($"Failed to create playlist {playlistName} because {playlist.Error.Message}");
                return;
            }

            var uris = tracks
                .OfType<SpotifyTrack>()
                .Select(x => x.SpotifyUri)
                .ToArray();
            for (int split = 0; split < uris.Length; split += 100)
            {
                var splitUris = new List<string>(100);
                for (int i = split; i < Math.Min(uris.Length, split + 100); i++)
                {
                    splitUris.Add(uris[i]);
                }
                var response = await _spotify.AddPlaylistTracksAsync(user.Id, playlist.Id, splitUris);
            }
        }
    }
}