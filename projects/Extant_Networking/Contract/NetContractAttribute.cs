using System;

using Extant.Extensions;

namespace Extant.Net.Contract
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NetContractAttribute : Attribute
    {
        public int ID { get; private set; }

        public NetContractAttribute(int id)
        {
            this.ID = id;
        }
    }
}
