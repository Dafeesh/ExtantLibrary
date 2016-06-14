using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Collections
{
    public class ListTable<TKey, TValue>
    {
        Dictionary<TKey, List<TValue>> _items = new Dictionary<TKey, List<TValue>>();

        ///////////////////////

        public new string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (var kvp in _items)
            {
                str.AppendLine(kvp.Key.ToString() + ":");
                foreach (var val in kvp.Value)
                {
                    str.AppendLine("\t" + val.ToString());
                }
            }
            return str.ToString();
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _items.ContainsKey(key);
        }

        /// <returns>True if a key was added.</returns>
        public bool AddOrSet(TKey key, TValue value)
        {
            if (!this.ContainsKey(key))
            {
                _items.Add(key, new List<TValue>());
                return true;
            }
            else
            {
                _items[key].Add(value);
                return false;
            }
        }

        public bool Remove(TKey key)
        {
            return _items.Remove(key);
        }

        public bool Remove(TKey key, TValue value)
        {
            return
                this.ContainsKey(key)
                &&
                _items[key].Remove(value);
        }

        public bool RemoveAll(TKey key, TValue value)
        {
            return
                this.ContainsKey(key)
                &&
                _items[key].RemoveAll((v) => v.Equals(value)) > 0;
        }

        public int Count()
        {
            int count = 0;
            foreach (var kvp in _items)
            {
                count += kvp.Value.Count;
            }
            return count;
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return _items.Keys;
            }
        }
    }
}