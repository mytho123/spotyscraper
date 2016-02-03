﻿using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
using SpotyScraper.Spotify.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpotyScraper.Spotify
{
    [Export(typeof(IStreamService))]
    [ExportMetadata(nameof(IStreamServiceData.Name), NAME)]
    [ExportMetadata(nameof(IStreamServiceData.Description), DESCRIPTION)]
    internal class SpotifyService : IStreamService
    {
        public const string NAME = "Spotify";
        public const string DESCRIPTION = "Spotify streaming service";

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
                Scope = Scope.PlaylistReadPrivate,
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

        public async Task ResolveAsync(IEnumerable<Track> tracks)
        {
            if (!await this.CheckAuthentication())
                return;

            foreach (var track in tracks)
            {
                await this.ResolveAsync(track);
            }
        }

        public async Task ResolveAsync(Track track)
        {
            var artist = track.Artists.FirstOrDefault();
            var q = $"track:\"{track.Title}\"&artist:\"{artist}\"";

            var searchItem = await _spotify.SearchItemsAsync(q, SearchType.Track);

            var spotifyTracks = searchItem?.Tracks?.Items.Select(x => new SpotifyTrack(x));
            if (spotifyTracks != null)
                track.SetMatches(spotifyTracks);
        }
    }
}