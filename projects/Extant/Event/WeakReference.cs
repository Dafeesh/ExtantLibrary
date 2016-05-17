using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Event
{
    public class WeakReference<T> : System.WeakReference where T : class
    {
        public WeakReference(T obj)
            : base(obj)
        { }

        public T TypedTarget
        {
            get
            {
                return this.Target as T;
            }
        }
    }
}
