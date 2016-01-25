using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Extensions
{
    public static class DictionaryExtensions
    {
        public static void SetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
