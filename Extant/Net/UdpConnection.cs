using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Extant.Logging;

namespace Extant.Net
{
    public class UdpConnection : ILogging
    {
        private UdpClient _udpClient;
        private IPEndPoint _remoteEndPoint;
        private IPEndPoint _localEndPoint;

        private NetPacket.PacketDecryptor _packetDecryptor;
        private Queue<NetPacket> _packets = new Queue<NetPacket>();
        private object _packets_lock = new object();

        private object _sendLock = new object();

        private Byte[] _receiveBuffer = new Byte[1024];

        private DebugLogger _log;
        private ByteRecord _byteLog_out = new ByteRecord();
        private ByteRecord _byteLog_in = new ByteRecord();
        private bool _isClosed = false;

        public UdpConnection(UdpClient udpClient, NetPacket.PacketDecryptor packetDecryptor)
        {
            this._log = new DebugLogger("UdpConnection");

            this._udpClient = udpClient;
            this._remoteEndPoint = (IPEndPoint)udpClient.Client.RemoteEndPoint;
            this._localEndPoint = (IPEndPoint)udpClient.Client.LocalEndPoint;
            this._packetDecryptor = packetDecryptor;

            BeginReceive();
        }

        /// <summary>
        /// Closes an active connection.
        /// </summary>
        public void Close()
        {
            if (!IsClosed)
            {
                this._udpClient.Close();

                Log.LogMessage("Closed.");
            }
        }

        private void BeginReceive()
        {
            this._udpClient.Client.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, Callback_Receive, this._udpClient.Client);
        }

        private void Callback_Receive(IAsyncResult ar)
        {
            if (!IsClosed)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;

                    int numBytes = client.EndReceive(ar);
                    if (numBytes == 0)
                    {
                        Log.LogMessage("ReceiveCallback, lost connection.");
                        this.Close();
                    }
                    else
                    {
                        _byteLog_in.Bytes += numBytes;

                        this.InterpretReceiveBuffer(_receiveBuffer, numBytes);

                        Log.LogMessage("ReceiveCallback, received bytes- " + numBytes + " -> " + _packets.Count);
                        BeginReceive();
                    }
                }
                catch (ObjectDisposedException)
                {
                    Log.LogMessage("ReceiveCallback, disposed.");
                }
                catch (Exception e)
                {
                    Log.LogMessage("ReceiveCallback exception: " + e.ToString());
                    this.Close();
                }
            }
        }

        private void InterpretReceiveBuffer(byte[] receiveBuffer, int numBytes)
        {
            if (receiveBuffer.Length > NetPacket.PacketPrefixLength)
            {
                byte[] buffer = receiveBuffer.Take(numBytes).ToArray();
                int bytesRead = 0;

                if (buffer.Length > NetPacket.PacketPrefixLength)
                {
                    byte packetId = buffer[0];
                    int packetLength = NetPacket.PacketPrefixLength + BitConverter.ToInt32(buffer, 1);

                    NetPacket packet = _packetDecryptor(packetId, buffer.Skip(1).Take(packetLength - 1).ToArray());
                    if (packet == null)
                    {
                        throw new FormatException("Error while interpretting packet from receivebuffer.");
                    }

                    bytesRead += packetLength;

                    lock (_packets_lock)
                    {
                        _packets.Enqueue(packet);
                    }
                }
                else
                {
                    Log.LogError("Invalid packet length!");
                }
            }
        }

        /// <summary>
        /// Returns the oldest packet in the queue.
        /// If no packets available then this returns null.
        /// </summary>
        public NetPacket GetPacket()
        {
            lock (_packets_lock)
            {
                if (_packets.Count > 0)
                {
                    return _packets.Dequeue();
                }
            }
            return null;
        }

        /// <summary>
        /// Sends a packet to this client.
        /// </summary>
        public void SendPacket(NetPacket p)
        {
            try
            {
                Byte[] d = p.GetBytes();
                lock (_sendLock)
                {
                    _byteLog_out.Bytes += d.Length;
                    _udpClient.Send(d, d.Length);
                }
            }
            catch (Exception e)
            {
                Log.LogMessage("Exception while while sending packet: " + e.Message);
                this.Close();
            }
        }

        /// <summary>
        /// True if a packet is ready to received via .GetPacket().
        /// </summary>
        public bool PacketAvailable
        {
            get
            {
                lock (_packets_lock)
                {
                    return _packets.Count > 0;
                }
            }
        }

        /// <summary>
        /// True if the connection is currently still being used.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (!this.IsClosed || this.PacketAvailable);
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return _remoteEndPoint;
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return _localEndPoint;
            }
        }

        public ILogger Log
        {
            get
            {
                return _log;
            }
        }

        public bool IsClosed
        {
            get
            {
                return _isClosed;
            }
        }
    }
}
