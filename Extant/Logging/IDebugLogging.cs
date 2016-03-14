using System;

namespace Extant.Logging
{
    public interface IDebugLogging
    {
        IDebugLogger Log
        {
            get;
        }
    }
}
