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

        public static string ToAggregateString(this IEnumerable<object> arr, bool withNewlines = true)
        {
            StringBuilder str = new StringBuilder();
            foreach (object o in arr)
            {
                if (withNewlines)
                    str.AppendLine(o.ToString());
                else
                    str.Append(o.ToString());
            }
            return str.ToString();
        }
    }
}
