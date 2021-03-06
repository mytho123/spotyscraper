﻿using DuoVia.FuzzyStrings;
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
        public static double MinimumScoreToMatch { get; set; } = .1;

        public Track(string title, string[] artists)
        {
            this.Title = title;
            this.Artists = artists ?? new string[0];
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { this.Set(ref _title, value); }
        }

        private string[] _artists;

        public string[] Artists
        {
            get { return _artists; }
            set { this.Set(ref _artists, value); }
        }

        private IReadOnlyDictionary<ITrackMatch, double> _matches;

        public IReadOnlyDictionary<ITrackMatch, double> Matches
        {
            get { return _matches; }
            set { this.Set(ref _matches, value); }
        }

        private KeyValuePair<ITrackMatch, double> _selectedMatch;

        public KeyValuePair<ITrackMatch, double> SelectedMatch
        {
            get { return _selectedMatch; }
            set { this.Set(ref _selectedMatch, value); }
        }

        public void SetMatches(IEnumerable<ITrackMatch> matches)
        {
            this.SelectedMatch = new KeyValuePair<ITrackMatch, double>();

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

            var bestMatch = this.Matches
                .OrderByDescending(x => x.Value)
                .FirstOrDefault();

            if (bestMatch.Value >= MinimumScoreToMatch)
            {
                this.SelectedMatch = bestMatch;
            }
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
    }

    public class TrackComparer : IEqualityComparer<Track>
    {
        public bool Equals(Track x, Track y)
        {
            return object.Equals(x.Title, y.Title)
                && x.Artists.Length == y.Artists.Length
                && string.Concat(x.Artists.OrderBy(t => t)) == string.Concat(y.Artists.OrderBy(t => t));
        }

        public int GetHashCode(Track obj)
        {
            return obj.Title.GetHashCode() ^ obj.Artists
                .OrderBy(x => x)
                .Select(x => x.GetHashCode())
                .Aggregate((x1, x2) => x1 ^ x2);
        }
    }
}