using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Extant.Logging;

namespace Extant.Net
{
    /// <summary>
    /// A network connection communicating with NetPackets.
    /// </summary>
    public interface INetConnection : IDebugLogging
    {
        void Send(NetPacket packet, NetworkProtocol protocol);
        NetPacket Receive();
        void Close();

        bool PacketAvailable { get; }
        bool IsActive { get; }
        long TimeSinceLastPacket { get; }
    }
}
