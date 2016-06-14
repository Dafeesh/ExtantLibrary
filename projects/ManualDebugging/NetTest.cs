using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Extant.Net;
using Extant.Net.Contract;
using Extant.Net.Host;
using Extant.Util;
using Extant.Threading;
using Extant.Net.Connect;
using Extant.Extensions;
using Extant.Logging;

namespace ExtantManualDebugging
{
    class NetTest
    {
        const int PacketID = 1;
        const int FromServerMessage = 111;
        const int FromClientMessage = 222;
        const int ListenTimeout = 3000;
        static readonly IPEndPoint HostEP = new IPEndPoint(IPAddress.Loopback, 12121);
        static readonly Type PacketGroup = typeof(SomePacketGroup);
        readonly IDebugLogger Log;

        public NetTest(IDebugLogger log)
        {
            this.Log = log;
        }

        public void Run()
        {
            //Start host
            Log.LogMessage("Starting...");

            if (!NetPacket.InitializeContractGroup(PacketGroup))
                return;

            var host = new NetTcpHost(HostEP, PacketGroup, PacketGroup, Log);
            var clientsRunning = new ThreadJob(_RunClientSwarm);
            try
            {
                //Start
                host.Start();
                Thread.Sleep(1000);
                clientsRunning.Start();

                //Wait
                Log.LogMessage("Waiting for first connection...");
                while (host.ActiveConnectionsCount == 0)
                { Thread.Sleep(1); }
                Log.LogMessage("---");

                //Listen until the end
                List<INetConnection> cons = new List<INetConnection>();
                INetConnection newClient;
                while (host.ActiveConnectionsCount > 0 && !(Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape))
                {
                    while ((newClient = host.PollNewConnection()) != null)
                    {
                        Log.LogMessage("New client: " + newClient.RemoteEndPoint.ToString());
                        cons.Add(newClient);
                    }

                    foreach (var c in cons)
                    {
                        if (c.PacketAvailable)
                        {
                            Log.LogMessage("[" + c.Log.SourceName + "]:");
                            while (c.PacketAvailable)
                            {
                                Log.LogMessage("\t" + ((SomePacketGroup.SomePacket)c.PollReceivePacket()).Number);
                            }
                        }
                    }

                    cons.RemoveAll((c) =>
                    {
                        if (!c.IsActive)
                        {
                            Log.LogMessage("Lost client: " + c.RemoteEndPoint.ToString());
                            c.Close(NetConnectionClosingReason.LostConnection);
                            return true;
                        }
                        else
                            return false;
                    });

                    Thread.Sleep(10);
                }
            }
            finally
            {
                //Stop
                try
                {
                    host.Stop(true);
                    clientsRunning.Join();
                }
                catch { }
            }
        }

        void _RunClientSwarm()
        {
            List<INetConnection> cons = new List<INetConnection>();
            ConnectResult conResult;
            for (int i = 0; i < 10; i++)
            {
                conResult = TcpConnecting.Connect(HostEP, PacketGroup, PacketGroup);
                if (conResult.Success)
                {
                    cons.Add(conResult.Connection);
                }
                else
                {
                    Log.LogMessage("Failed to connect - " + i);
                }

                Thread.Sleep(500);
            }

            NetPacket[] packets = new NetPacket[10];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new SomePacketGroup.SomePacket(i + 1);
            }

            foreach (var c in cons)
            {
                for (int i = 0; i < packets.Length; i++)
                {
                    c.Send(packets[i]);
                }
            }

            Thread.Sleep(4000);

            foreach (var c in cons)
            {
                c.Close();
            }
        }

        [NetContractGroup]
        class SomePacketGroup
        {
            [NetContract(1)]
            public class SomePacket : NetPacket
            {
                [NetContractMember(1)]
                public int Number { get; set; }

                public SomePacket(int n)
                    : this()
                {
                    Number = n;
                }

                protected SomePacket()
                    : base(1)
                { }
            }
        }
    }
}
