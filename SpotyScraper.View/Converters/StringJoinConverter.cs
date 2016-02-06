using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SpotyScraper.View.Converters
{
    internal class StringJoinConverter : IValueConverter
    {
        public static StringJoinConverter Instance { get; } = new StringJoinConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strings = value as IEnumerable<string>;
            if (strings != null)
            {
                var separator = parameter as string ?? ", ";
                return string.Join(separator, strings);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (str != null)
            {
                return str.Split(',').Select(x => x.Trim()).ToArray();
            }
            else
            {
                return value;
            }
        }
    }
}