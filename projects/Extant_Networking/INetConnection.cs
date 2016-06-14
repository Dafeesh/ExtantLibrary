using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Extant.Logging;
using Extant.Util;

namespace Extant.Net
{
    public enum NetConnectionClosingReason
    {
        None,
        ClosedSelf,
        LostConnection,
        PacketSerializeError,
        UnknownException
    }

    /// <summary>
    /// A network connection communicating via NetPackets.
    /// </summary>
    public interface INetConnection : IByteRecording, IDebugLogging
    {
        void Send(NetPacket packet);
        NetPacket PollReceivePacket();
        void Close(NetConnectionClosingReason closingReason = NetConnectionClosingReason.ClosedSelf, Exception unhandledException = null);

        bool PacketAvailable { get; }
        TimeSpan TimeSinceLastPacketReceived { get; }
        TimeSpan TimeSinceLastPacketSent { get; }
        TimeSpan TimeAlive { get; }

        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        bool IsActive { get; }
        NetConnectionClosingReason ClosingReason { get; }
        Exception UnhandledException { get; }
    }
}
