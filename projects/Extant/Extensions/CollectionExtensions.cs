using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Extensions
{
    public static class CollectionExtensions
    {
        public static bool HasNext<T>(this Queue<T> queue)
        {
            return queue.Count > 0;
        }
    }
}
