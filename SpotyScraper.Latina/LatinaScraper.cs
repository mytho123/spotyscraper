using HtmlAgilityPack;
using SpotyScraper.Model.Scrapers;
using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SpotyScraper.Latina
{
    [Export(typeof(IScraper))]
    [ExportMetadata(nameof(IScraperData.Name), NAME)]
    [ExportMetadata(nameof(IScraperData.Description), DESCRIPTION)]
    internal class LatinaScraper : IScraper
    {
        public const string NAME = "Latina scraper";
        public const string DESCRIPTION = "A scraper for Latina FM radio station";

        public string Name { get; } = NAME;
        public string Description { get; } = DESCRIPTION;

        public IEnumerable<Track> Scrap(IProgress<double> progress)
        {
            var funcs = this.GetAllPagesContentFuncs().ToArray();
            int nbDone = 0;

            foreach (var pageContent in funcs.Select(x => x()))
            {
                foreach (var track in this.ScrapPage(pageContent))
                {
                    yield return track;
                }
                progress.Report((double)nbDone++ / (double)funcs.Length);
            }
        }

        #region Request

        public const string FORM_URL = "http://www.latina.fr//index.php?id=16";
        public const string FORM_YEAR = "ahisto";
        public const string FORM_MONTH = "mhisto";
        public const string FORM_DAY = "jhisto";
        public const string FORM_HOUR = "hhisto";
        public const string FORM_MINUTES = "minhisto";

        private IEnumerable<Func<string>> GetAllPagesContentFuncs()
        {
            var now = DateTime.Now;

            for (double hour = 24 * 7; hour >= 0; hour -= .5)
            {
                var offset = TimeSpan.FromHours(hour);
                yield return () => this.GetPageContent(now - offset);
            }
        }

        private string GetPageContent(DateTime date)
        {
            var request = HttpWebRequest.CreateHttp(FORM_URL);
            request.Method = "POST";

            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.97 Safari/537.36";
            request.Referer = FORM_URL;
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Origin", "http://www.latina.fr");
            //request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.8,fr-FR;q=0.6,fr;q=0.4");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            var postBuilder = new StringBuilder();
            postBuilder.Append($"{FORM_YEAR}={date.Year.ToString("D2")}");
            postBuilder.Append($"&{FORM_MONTH}={date.Month.ToString("D2")}");
            postBuilder.Append($"&{FORM_DAY}={date.Day.ToString("D2")}");
            postBuilder.Append($"&{FORM_HOUR}={date.Hour.ToString("D2")}");
            postBuilder.Append($"&{FORM_MINUTES}={date.Minute.ToString("D2")}");

            //var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(postBuilder.ToString());
            //var data = Encoding.ASCII.GetBytes(postBuilder.ToString());
            var data = Encoding.UTF8.GetBytes(postBuilder.ToString());

            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            Debug.WriteLine($"{DateTime.Now} Latina scraper: {date} done");

            var encoding = Encoding.GetEncoding(response.CharacterSet);
            var result = new StreamReader(response.GetResponseStream(), encoding).ReadToEnd();
            return result;
        }

        #endregion Request

        private IEnumerable<Track> ScrapPage(string pageContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);
            foreach (var infoNode in GetDescendants(doc.DocumentNode, "div", "floatLeft DateResultat"))
            {
                var artistParent = infoNode.NextSibling?.NextSibling?.NextSibling?.NextSibling;
                var artist = artistParent?.FirstChild?.InnerText;

                var titleParent = artistParent?.NextSibling?.NextSibling;
                var title = titleParent?.FirstChild?.InnerText;

                if (title != null && artist != null)
                {
                    yield return new Track(title, new string[] { artist });
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