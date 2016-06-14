using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Threading
{
    public class LockValuePair<TValue>
    {
        public TValue Value { get; set; }
        public object Lock { get; private set; }

        public LockValuePair(TValue value)
        {
            //if (object.ReferenceEquals(value, null))
            //    throw new ArgumentNullException("Value's reference cannot be null.");

            this.Lock = new object();
            this.Value = value;
        }

        ////Doesn't feel right... Maybe come back to this..
        //public void SafelyRun(Action<TValue> a)
        //{
        //    lock (Lock)
        //    {
        //        a.Invoke(Value);
        //    }
        //}
    }
}
