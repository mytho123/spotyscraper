using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Scrapers
{
    public interface IScraper
    {
        string Name { get; }
        string Description { get; }

        IEnumerable<Track> Scrap();
    }
}