using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;

            return string.IsNullOrEmpty(value.Trim());
        }

        public static string ToAggregateString<T>(this IEnumerable<T> ienum, string separator = "\n", bool trailingSeparator = true)
        {
            StringBuilder str = new StringBuilder();
            T[] arr = ienum.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                str.Append(arr[i].ToString() + ((i + 1 < arr.Length || trailingSeparator) ? separator : ""));
            }
            return str.ToString();
        }
    }
}
