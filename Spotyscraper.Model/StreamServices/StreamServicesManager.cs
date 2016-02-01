using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.StreamServices
{
    public class StreamServicesManager
    {
        [ImportMany]
        private IEnumerable<Lazy<IStreamService, IStreamServiceData>> _services;

        private StreamServicesManager()
        {
            DiscoverScrapers();
        }

        public static StreamServicesManager Instance { get; } = new StreamServicesManager();

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
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public IEnumerable<IStreamServiceData> GetServicesData()
        {
            return _services.Select(x => x.Metadata);
        }

        public IStreamService GetService(IStreamServiceData serviceData)
        {
            return _services.FirstOrDefault(x => x.Metadata == serviceData).Value;
        }
    }
}