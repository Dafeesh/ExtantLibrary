using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;

using Extant.Logging;
using Extant.Util;
using Extant.Threading;

namespace Extant.Net
{
    public class NetStreamConnection : INetConnection
    {
        private LockValuePair<NetworkStream> _stream;

        private readonly IPEndPoint _remoteEndPoint;
        private readonly IPEndPoint _localEndPoint;

        private MemoryStream _receivedBytes = new MemoryStream();
        private byte[] _tempReceiveBuffer = new byte[1024];

        private PacketQueue _inboundPackets;
        private PacketQueue _outboundPackets;
        private bool _isSending = false;

        private SimpleTimer _lifeTimer = SimpleTimer.StartNew();
        private ByteRecorder _byteRecord = new ByteRecorder();
        private bool _isClosed = false;
        private NetConnectionClosingReason _closingReason = NetConnectionClosingReason.None;

        private Exception _unhandledException = null;
        private DebugLogger _log;

        public NetStreamConnection(NetworkStream client, Type inboundPacketGroup, Type outboundPacketGroup, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IDebugLogger parentLogger = null)
        {
            if (client == null || inboundPacketGroup == null || outboundPacketGroup == null)
                throw new ArgumentNullException();

            this._stream = new LockValuePair<NetworkStream>(client);
            this._remoteEndPoint = remoteEndPoint;
            this._localEndPoint = localEndPoint;
            this._inboundPackets = new PacketQueue(inboundPacketGroup);
            this._outboundPackets = new PacketQueue(outboundPacketGroup);
            this._log = new DebugLogger("NetStream{" + localEndPoint.Port + "->" + remoteEndPoint.ToString() + "}", parentLogger);

            BeginReceive();
        }

        private void BeginReceive()
        {
            try
            {
                lock (_stream.Lock)
                {
                    _stream.Value.BeginRead(_tempReceiveBuffer, 0, _tempReceiveBuffer.Length, ReceiveCallback, _stream.Value);
                }
            }
            catch (ObjectDisposedException)
            {
                Close(NetConnectionClosingReason.ClosedSelf);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                    Close(NetConnectionClosingReason.LostConnection, e);
                else
                    Close(NetConnectionClosingReason.UnknownException, e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (_isClosed)
                return;

            try
            {
                lock (_stream.Lock)
                {
                    int numReceivedBytes = ((NetworkStream)ar.AsyncState).EndRead(ar);
                    if (numReceivedBytes != 0)
                    {
                        _receivedBytes.Write(_tempReceiveBuffer, 0, numReceivedBytes);

                        NetPacket packet;
                        while (NetPacket.TryDeserialize(_receivedBytes, _inboundPackets.PacketGroup, out packet))
                        {
                            Log.LogMessage("In:\t[" + numReceivedBytes + "] " + packet.GetType().ToString());
                            _inboundPackets.Enqueue(packet);
                        }

                        _byteRecord.AddInboundBytes(numReceivedBytes);
                        BeginReceive();
                    }
                    else
                        Close(NetConnectionClosingReason.LostConnection);
                }
            }
            catch (FormatException e)
            {
                Close(NetConnectionClosingReason.PacketSerializeError, e);
            }
            catch (ObjectDisposedException e)
            {
                Close(NetConnectionClosingReason.ClosedSelf, e);
            }
            catch (Exception e)
            {
                Close(NetConnectionClosingReason.LostConnection, e);
            }
        }

        private void BeginSend()
        {
            if (_isClosed)
                return;

            try
            {
                lock (_stream.Lock)
                {
                    if (!_isSending)
                    {
                        if (_outboundPackets.HasNext)
                        {
                            NetPacket packet = _outboundPackets.PollDequeue();
                            byte[] bytesToSend = packet.Serialize();

                            _stream.Value.BeginWrite(bytesToSend, 0, bytesToSend.Length, SendCallback, _stream.Value);
                            _byteRecord.AddOutboundBytes(bytesToSend.Length);
                            _isSending = true;

                            Log.LogMessage("Out:\t[" + bytesToSend.Length + "] " + packet.GetType().ToString());
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                Close(NetConnectionClosingReason.PacketSerializeError, e);
            }
            catch (ObjectDisposedException)
            {
                Close(NetConnectionClosingReason.ClosedSelf);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                    Close(NetConnectionClosingReason.LostConnection, e);
                else
                    Close(NetConnectionClosingReason.UnknownException, e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (_isClosed)
                return;

            try
            {
                lock (_stream.Lock)
                {
                    ((NetworkStream)ar.AsyncState).EndWrite(ar);
                    _isSending = false;
                    BeginSend();
                }
            }
            catch (FormatException e)
            {
                Close(NetConnectionClosingReason.PacketSerializeError, e);
            }
            catch (ObjectDisposedException)
            {
                Close(NetConnectionClosingReason.ClosedSelf);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                    Close(NetConnectionClosingReason.LostConnection, e);
                else
                    Close(NetConnectionClosingReason.UnknownException, e);
            }
        }

        /// <summary>
        /// Returns the oldest packet in the queue.
        /// If no packets available then this returns null.
        /// </summary>
        public NetPacket PollReceivePacket()
        {
            return _inboundPackets.PollDequeue();
        }

        /// <summary>
        /// Send a packet to this client.
        /// </summary>
        public void Send(NetPacket p)
        {
            if (_isClosed)
                return;

            try
            {
                lock (_stream.Lock)
                {
                    _outboundPackets.Enqueue(p);
                    BeginSend();
                }
            }
            catch (Exception e)
            {
                Close(NetConnectionClosingReason.UnknownException, e);
            }
        }

        /// <summary>
        /// Closes an engaged connection. Connection will still be considered 'Active' while incoming packets are still
        /// stored in the buffer.
        /// </summary>
        public void Close(NetConnectionClosingReason closingReason = NetConnectionClosingReason.ClosedSelf, Exception unhandledException = null)
        {
            if (_isClosed)
                return;

            try
            {
                lock (_stream.Lock)
                {
                    _stream.Value.Close();

                    _unhandledException = unhandledException;
                    _isClosed = true;
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Exception while trying to close: " + e.ToString());
            }
            finally
            {
                if (_closingReason == NetConnectionClosingReason.None)
                    _closingReason = closingReason;
                _lifeTimer.Stop();
                Log.LogMessage("Closed [" + closingReason.ToString() + "]" + (unhandledException != null ? (" Exception: " + unhandledException.ToString()) : ""));
            }
        }

        /// <summary>
        /// True if a packet is ready to be received.
        /// </summary>
        public bool PacketAvailable
        { get { return _inboundPackets.HasNext; } }

        /// <summary>
        /// The time since this connection received a valid packet.
        /// </summary>
        public TimeSpan TimeSinceLastPacketReceived
        { get { return _inboundPackets.TimeSinceLastEnqueue; } }

        /// <summary>
        /// The time since this connection received a valid packet.
        /// </summary>
        public TimeSpan TimeSinceLastPacketSent
        { get { return _outboundPackets.TimeSinceLastEnqueue; } }

        /// <summary>
        /// The amount of time this connection has been alive for.
        /// Time halts After connection is closed.
        /// </summary>
        public TimeSpan TimeAlive
        { get { return _lifeTimer.Elapsed; } }

        /// <summary>
        /// True if the connection is currently still being used.
        /// </summary>
        public bool IsActive
        { get { return (!_isClosed || this.PacketAvailable); } }

        public bool IsClosed
        { get { return _isClosed; } }

        public IPEndPoint RemoteEndPoint
        { get { return _remoteEndPoint; } }

        public IPEndPoint LocalEndPoint
        { get { return _localEndPoint; } }

        public NetConnectionClosingReason ClosingReason
        { get { return _closingReason; } }

        public IByteRecord ByteRecord
        { get { return _byteRecord; } }

        public Exception UnhandledException
        { get { return _unhandledException; } }

        public IDebugLogger Log
        { get { return _log; } }
    }
}