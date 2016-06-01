using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Unity.Caching
{
    internal class ObjectLifePair
    {
        public UnityEngine.Object Object { get; private set; }
        public long Generation { get; private set; }

        public ObjectLifePair(UnityEngine.Object obj)
        {
            this.Object = obj;
            this.Generation = 0;
        }

        public void IncreaseGeneration()
        {
            this.Generation++;
        }
    }
}
