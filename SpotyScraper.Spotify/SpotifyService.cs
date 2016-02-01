using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
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

        private readonly SpotifyWebAPI _spotify;

        public SpotifyService()
        {
            _spotify = new SpotifyAPI.Web.SpotifyWebAPI();
        }

        public string Name { get; } = NAME;

        public string Description { get; } = DESCRIPTION;

        public async Task ResolveAsync(IEnumerable<Track> tracks)
        {
            //var tasks = new List<Task>();
            foreach (var track in tracks)
            {
                await this.ResolveAsync(track);
            }
            //await Task.WaitAll(tasks.ToArray());
        }

        public async Task ResolveAsync(Track track)
        {
            var q = $"track:\"{track.Title}\"&artist:\"{track.Artist}\"";
            var searchItem = await _spotify.SearchItemsAsync(q, SearchType.Track);
        }
    }
}