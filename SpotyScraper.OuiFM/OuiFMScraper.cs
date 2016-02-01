using SpotyScraper.Model.Scrapers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.OuiFM
{
    [Export(typeof(IScraper))]
    [ExportMetadata(nameof(IScraperData.Name), NAME)]
    [ExportMetadata(nameof(IScraperData.Description), DESCRIPTION)]
    internal class OuiFMScraper : IScraper
    {
        public const string NAME = "Oui FM scraper";
        public const string DESCRIPTION = "A scraper for Oui FM radio station";

        public string Name { get; } = NAME;
        public string Description { get; } = DESCRIPTION;
    }
}