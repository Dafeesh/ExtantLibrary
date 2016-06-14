using System;
using System.Net.Sockets;

namespace Extant.Net.Connect
{
    public class ConnectResult
    {
        public bool Success { get { return this.UnhandledException == null; } }
        public INetConnection Connection { get; private set; }
        public Exception UnhandledException { get; private set; }

        public ConnectResult(INetConnection con, Exception error = null)
        {
            this.Connection = con;
            this.UnhandledException = error;
        }
    }
}
