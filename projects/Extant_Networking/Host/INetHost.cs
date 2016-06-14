using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Extant.Logging;
using Extant.Util;

namespace Extant.Net.Host
{
    public interface INetHost : IDebugLogging
    {
        void Start();
        void Close();
        INetConnection PollNewConnection();
        
        IPEndPoint LocalEndPoint { get; }
    }
}
