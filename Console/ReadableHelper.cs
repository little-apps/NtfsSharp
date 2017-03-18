using System.Collections.Generic;
using System.Linq;

namespace Console
{
    internal static class ReadableHelper
    {

        internal static string MakeReadable(this IEnumerable<byte> bytes)
        {
            var str = bytes.Aggregate("", (current, b) => current + $"0x{b:X}, ");
            return str.Substring(0, str.Length - 2);
        }

        internal static string MakeReadable(this IEnumerable<char> chars)
        {
            var str = chars.Aggregate("", (current, ch) => current + $"'{ch}', ");

            return str.Substring(0, str.Length - 2);
        }
    }
}
