using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.StreamServices
{
    public interface IStreamService
    {
        string Name { get; }
        string Description { get; }

        Task ResolveAsync(IEnumerable<Track> tracks);

        Task CreatePlaylist(string playlistName, IEnumerable<ITrackMatch> tracks);
    }
}