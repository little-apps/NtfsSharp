using System;
using System.Globalization;
using System.Windows.Data;

namespace DiskUsage
{
    [ValueConversion(typeof(object), typeof(string))]
    public class BytesToReadableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var bytes = ulong.Parse(value.ToString());

            return SizeToString(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string SizeToString(ulong size)
        {
            double len = size;

            var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
