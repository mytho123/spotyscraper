using HtmlAgilityPack;
using SpotyScraper.Model.Scrapers;
using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.OuiFM
{
    [Export(typeof(IScraper))]
    [ExportMetadata(nameof(IScraperData.Name), NAME)]
    [ExportMetadata(nameof(IScraperData.Description), DESCRIPTION)]
    internal class VoltageScraper : IScraper
    {
        public const string NAME = "Voltage scraper";
        public const string DESCRIPTION = "A scraper for voltage radio station";

        public string Name { get; } = NAME;
        public string Description { get; } = DESCRIPTION;

        public IEnumerable<Track> Scrap(IProgress<double> progress)
        {
            throw new NotImplementedException();
        }
    }
}