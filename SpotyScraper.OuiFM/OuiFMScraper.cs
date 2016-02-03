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
    internal class OuiFMScraper : IScraper
    {
        public const string NAME = "Oui FM scraper";
        public const string DESCRIPTION = "A scraper for Oui FM radio station";

        private const string EDITOUIFM = " (edit oui fm)";

        public string Name { get; } = NAME;
        public string Description { get; } = DESCRIPTION;

        public IEnumerable<Track> Scrap()
        {
            foreach (var pageURL in this.GetAllPagesURL())
            {
                Debug.WriteLine($"{DateTime.Now} Oui FM scraper: {pageURL.Substring(79)}");
                foreach (var track in this.ScrapPage(pageURL))
                {
                    yield return track;
                }
            }
        }

        private IEnumerable<string> GetAllPagesURL()
        {
            var now = DateTime.Now;

            for (int hour = 24 * 7; hour >= 0; hour--)
            {
                var offset = TimeSpan.FromHours(hour);
                yield return this.GetPageURL(now - offset);
            }
        }

        private string GetPageURL(DateTime date)
        {
            var month = date.Month.ToString("D2");
            var day = date.Day.ToString("D2");
            return $"http://www.ouifm.fr/wp-admin/admin-ajax.php?action=get_piges_results&flux=rock&date={date.Year}-{month}-{day}&time={date.Hour}";
        }

        private IEnumerable<Track> ScrapPage(string pageURL)
        {
            var request = WebRequest.CreateHttp(pageURL);
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                foreach (var infoNode in GetDescendants(doc.DocumentNode, "div", "info"))
                {
                    var artistNode = GetDescendants(infoNode, "strong", "artist").FirstOrDefault();
                    var titleNode = GetDescendants(infoNode, "strong", "title").FirstOrDefault();

                    if (artistNode != null && titleNode != null)
                    {
                        var artists = new string[] { artistNode.InnerText };
                        var title = titleNode.InnerText;

                        if (title.ToLowerInvariant().EndsWith(EDITOUIFM))
                            title = title.Remove(title.Length - EDITOUIFM.Length);

                        yield return new Track(title, artists);
                    }
                }
            }
        }

        private static IEnumerable<HtmlNode> GetDescendants(HtmlNode node, string nodeName, string nodeClass)
        {
            return node.Descendants()
                .Where(x => x.Name == nodeName)
                .Where(x => x.GetAttributeValue("class", (string)null) == nodeClass);
        }
    }
}