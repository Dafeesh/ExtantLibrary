using System;

namespace Extant.Net.Contract
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NetContractMemberAttribute : Attribute
    {
        public int Order { get; private set; }

        public NetContractMemberAttribute(int order)
        {
            this.Order = order;
        }
    }
}
