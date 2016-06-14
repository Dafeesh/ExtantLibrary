using System;

namespace Extant.Net.Contract
{
    public class InvalidContractException : Exception
    {
        public InvalidContractException(string message, Exception innerException = null)
            : base(message, innerException)
        { }
    }
}
