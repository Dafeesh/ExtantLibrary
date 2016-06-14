using System;
using System.Collections.Generic;
using System.Linq;

using Extant.Collections;

namespace Extant.Extensions
{
    public static class EnumerableExtensions
    {
        public static ListTable<TKey, TValue> ToListTable<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            ListTable<TKey, TValue> table = new ListTable<TKey, TValue>();
            foreach (var kvp in values)
            {
                table.AddOrSet(kvp.Key, kvp.Value);
            }
            return table;
        }
    }
}
