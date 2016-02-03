using DuoVia.FuzzyStrings;
using SpotyScraper.Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Model.Tracks
{
    public class Track : PropertyChangeNotifier
    {
        public Track(string title, string[] artists)
        {
            this.Title = title;
            this.Artists = artists ?? new string[0];
        }

        public string Title { get; }
        public string[] Artists { get; }

        private IReadOnlyDictionary<ITrackMatch, double> _matches;

        public IReadOnlyDictionary<ITrackMatch, double> Matches
        {
            get { return _matches; }
            set { this.Set(ref _matches, value); }
        }

        private ITrackMatch _selectedMatch;

        public ITrackMatch SelectedMatch
        {
            get { return _selectedMatch; }
            set
            {
                this.Set(ref _selectedMatch, value);
                this.RaisePropertyChanged(nameof(this.SelectedMatchScore));
            }
        }

        public double SelectedMatchScore
        {
            get { return this.Matches != null && this.Matches.ContainsKey(this.SelectedMatch) ? this.Matches[this.SelectedMatch] : double.NaN; }
        }

        public void SetMatches(IEnumerable<ITrackMatch> matches)
        {
            var result = new Dictionary<ITrackMatch, double>();
            foreach (var match in matches)
            {
                var score = new double[]
                    {
                        InverseLevenshtein(match.Title, this.Title),
                        InverseLevenshtein(GetArtistsString(match.Artists), GetArtistsString(this.Artists)),
                    }.Average();
                result[match] = score;
            }
            this.Matches = new ReadOnlyDictionary<ITrackMatch, double>(result);

            this.SelectedMatch = this.Matches
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

        public override string ToString()
        {
            return $"{this.Title} - {string.Join(", ", this.Artists)}";
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode() ^ this.Artists
                .OrderBy(x => x)
                .Select(x => x.GetHashCode())
                .Aggregate((x1, x2) => x1 ^ x2);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Track;
            if (other == null)
                return false;

            return object.Equals(this.Title, other.Title)
                && this.Artists.Length == other.Artists.Length
                && string.Concat(this.Artists.OrderBy(x => x)) == string.Concat(other.Artists.OrderBy(x => x));
        }
    }
}