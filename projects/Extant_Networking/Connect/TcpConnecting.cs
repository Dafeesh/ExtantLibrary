using System;
using System.Net;
using System.Net.Sockets;

using Extant.Threading;
using Extant.Logging;

namespace Extant.Net.Connect
{
    public static class TcpConnecting
    {
        public static ConnectResult Connect(IPEndPoint remoteEndPoint, Type inboundPacketGroup, Type outboundPacketGroup, IDebugLogger parentLogger = null)
        {
            if (remoteEndPoint == null || inboundPacketGroup == null || outboundPacketGroup == null)
                throw new ArgumentNullException();

            TcpClient client = null;
            try
            {
                client = new TcpClient();
                client.Connect(remoteEndPoint);
                return new ConnectResult(
                    new NetStreamConnection(
                        client.GetStream(), 
                        inboundPacketGroup, outboundPacketGroup,
                        (IPEndPoint)client.Client.LocalEndPoint,
                        (IPEndPoint)client.Client.RemoteEndPoint, 
                        parentLogger));
            }
            catch (Exception e)
            {
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch { }
                }
                return new ConnectResult(null, e);
            }
            finally
            {
            }
        }

        public static IThreadJob<ConnectResult> ConnectAsync(IPEndPoint remoteEndPoint, Type inboundPacketGroup, Type outboundPacketGroup, IDebugLogger parentLogger = null)
        {
            var thr = new ThreadJob<ConnectResult>(() => Connect(remoteEndPoint, inboundPacketGroup, outboundPacketGroup, parentLogger));
            thr.Start();
            return thr;
        }
    }
}
