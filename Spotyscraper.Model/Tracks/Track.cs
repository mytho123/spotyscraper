using DuoVia.FuzzyStrings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Tracks
{
    public class Track
    {
        private readonly Dictionary<ITrackMatch, double> _matches;

        public Track(string title, string[] artists)
        {
            this.Title = title;
            this.Artists = artists;

            _matches = new Dictionary<ITrackMatch, double>();
            this.Matches = new ReadOnlyDictionary<ITrackMatch, double>(_matches);
        }

        public string Title { get; }
        public string[] Artists { get; }
        public IReadOnlyDictionary<ITrackMatch, double> Matches { get; }
        public ITrackMatch SelectedMatch { get; set; }

        public void SetMatches(IEnumerable<ITrackMatch> matches)
        {
            _matches.Clear();

            foreach (var match in matches)
            {
                var score = new double[]
                    {
                        InverseLevenshtein(match.Title, this.Title),
                        InverseLevenshtein(GetArtistsString(match.Artists), GetArtistsString(this.Artists)),
                    }.Average();
                _matches[match] = score;
            }

            this.SelectedMatch = _matches
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        private static double InverseLevenshtein(string str1, string str2)
        {
            var levenshtein = str1.LevenshteinDistance(str2);
            return 1 / Math.Max(1.0, (double)levenshtein);
        }

        private static string GetArtistsString(IEnumerable<string> artists)
        {
            return string.Join(",", artists);
        }
    }
}