using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.Utils
{
    public static class CollectionsExtensions
    {
        public static void AddRange<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }
        }
    }
}