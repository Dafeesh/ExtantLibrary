using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Collections
{
    public class CacheList<T> : IEnumerable<T>
    {
        private List<T> _items = new List<T>();
        private int _size = 0;

        public void Resize(int size, Func<T> initializeDefault)
        {
            while (_items.Count < size)
            {
                _items.Add(initializeDefault());
            }

            _size = size;
        }

        public T this[int index]
        {
            get
            {
                return _items[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Take(_size).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.Take(_size).GetEnumerator();
        }
    }
}
