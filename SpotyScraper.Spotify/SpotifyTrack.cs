using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
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
    internal class SpotifyTrack : ITrackMatch
    {
        public SpotifyTrack(FullTrack track)
        {
            this.Title = track.Name;
            this.Artists = track.Artists.Select(x => x.Name).ToArray();
            this.SpotifyId = track.Id;
            this.Popularity = track.Popularity;
            this.Album = track.Album.Name;
        }

        public string Title { get; }
        public string[] Artists { get; }
        public string Album { get; }
        public string SpotifyId { get; }
        public int Popularity { get; }

        public override string ToString()
        {
            return $"{this.Title} - {string.Join(", ", this.Artists)} - {this.SpotifyId}";
        }
    }
}