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

        public static void SetOrAddAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            foreach (var kvp in values)
            {
                dictionary.SetOrAdd(kvp.Key, kvp.Value);
            }
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> ifNotExist)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, ifNotExist.Invoke());

            return dictionary[key];
        }
    }
}
