using System;

namespace Extant.Logging
{
    public interface ILogging
    {
        ILogger Log
        {
            get;
        }
    }
}
