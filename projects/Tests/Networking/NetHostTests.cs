using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Extant.Net;
using Extant.Net.Host;
using Extant.Util;
using Extant.Net.Connect;
using Extant.Net.Contract;

namespace ExtantTests.Networking
{
    [TestClass]
    public class NetHostTests
    {
        const int PacketID = 99;
        const int FromServerMessage = 111;
        const int FromClientMessage = 222;
        const int ListenTimeout = 3000;
        static readonly Type PacketGroup = typeof(SomePacketGroup);

        [TestMethod]
        public void NetTcpHost_SimpleTest()
        {
            NetPacket.InitializeContractGroup(typeof(SomePacketGroup));

            IPEndPoint HostEP = new IPEndPoint(IPAddress.Loopback, NextPort());

            //Start host
            var host = new NetTcpHost(HostEP, PacketGroup, PacketGroup);
            host.Start();

            //Start connection from client
            var clientConnecting = TcpConnecting.ConnectAsync(HostEP, PacketGroup, PacketGroup);

            //Receive client
            SimpleTimer timer = SimpleTimer.StartNew();
            INetConnection hostClient;
            while ((hostClient = host.PollNewConnection()) == null)
            {
                if (timer.ElapsedMilliseconds > ListenTimeout)
                    Assert.Fail("Host failed to notice connecting client.");
            }

            //Client recognize server
            if (clientConnecting.Result().Success)
            {
                var client = clientConnecting.Result().Connection;

                //Client send
                client.Send(new SomePacketGroup.SomePacket(FromClientMessage));

                //Host receive
                timer.Restart();
                while (!hostClient.PacketAvailable)
                {
                    if (timer.ElapsedMilliseconds > ListenTimeout)
                        Assert.Fail("Server failed to receive packet.");
                }
                var serverPacket = (SomePacketGroup.SomePacket)hostClient.PollReceivePacket();
                if (serverPacket.PacketID != PacketID)
                    Assert.Fail("Packet received by host ID invalid: " + serverPacket.PacketID);
                if (serverPacket.Number != FromClientMessage)
                    Assert.Fail("Packet received by host number invalid: " + serverPacket.Number);

                //Host send
                hostClient.Send(new SomePacketGroup.SomePacket(FromServerMessage));

                //Client receive
                timer.Restart();
                while (!client.PacketAvailable)
                {
                    if (timer.ElapsedMilliseconds > ListenTimeout)
                        Assert.Fail("Client failed to receive server packet.");
                }
                var clientPacket = (SomePacketGroup.SomePacket)client.PollReceivePacket();
                if (clientPacket.PacketID != PacketID)
                    Assert.Fail("Packet received by client ID invalid: " + clientPacket.PacketID);
                if (clientPacket.Number != FromServerMessage)
                    Assert.Fail("Packet received by client number invalid: " + clientPacket.Number);

                host.Stop(true);
                Console.WriteLine("Success!");
            }
            else
            {
                Assert.Fail("Client failed to recognize connect: " + clientConnecting.Result().UnhandledException.ToString());
            }
        }

        [NetContractGroup]
        class SomePacketGroup
        {
            [NetContract(99)]
            public class SomePacket : NetPacket
            {
                [NetContractMember(1)]
                public int Number;

                public SomePacket(int n)
                    : this()
                {
                    Number = n;
                }

                protected SomePacket()
                    : base(99)
                { }
            }
        }

        int NextPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
