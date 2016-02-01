using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Scrapers
{
    public class ScrapersManager
    {
        [ImportMany]
        private IEnumerable<Lazy<IScraper, IScraperData>> _scrapers;

        private ScrapersManager()
        {
            DiscoverScrapers();
        }

        public static ScrapersManager Instance { get; } = new ScrapersManager();

        private void DiscoverScrapers()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var exeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            catalog.Catalogs.Add(new DirectoryCatalog(exeDirectory));

            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        public IEnumerable<IScraperData> GetScrapersData()
        {
            return _scrapers.Select(x => x.Metadata);
        }

        public IScraper GetScraper(IScraperData scraperData)
        {
            return _scrapers.FirstOrDefault(x => x.Metadata == scraperData).Value;
        }
    }
}