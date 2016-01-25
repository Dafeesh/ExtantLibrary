using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Extensions
{
    public static class NumberExtentions
    {
        public static T AsPositiveOrDefault<T>(this T value) where T : IComparable
        {
            return (value.CompareTo(default(T)) <= 0) ? default(T) : value;
        }
    }
}
