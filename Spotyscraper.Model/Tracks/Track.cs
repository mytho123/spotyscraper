using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Tracks
{
    public class Track
    {
        public Track(string title, string artist)
        {
            this.Title = title;
            this.Artist = artist;
        }

        public string Title { get; }
        public string Artist { get; }
    }
}