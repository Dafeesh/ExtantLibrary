using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Extant;
using Extant.Net;
using Extant.Extensions;

namespace ExtantManualDebugging
{
    class StreamTest
    {
        MemoryStream stream = new MemoryStream(10);

        public void Run()
        {
            for (int i = 0; i < 12; i++)
            {
                Console.WriteLine("----");
                Put((byte)i);
                //Read();
                PrintState();

                if (i == 5)
                    stream.Shift(100);
            }
        }

        void Put(byte b)
        {
            stream.WriteByte(b);
        }

        byte[] hugeBuffer = new byte[100000];
        void Read()
        {
            Console.Write("Pos: " + stream.Position + " -> ");
            stream.MoveToStart();
            int didRead = stream.Read(hugeBuffer, 0, hugeBuffer.Length);
            stream.MoveToEnd();
            Console.Write("[" + didRead + "] ");
            Console.WriteLine(stream.Position);
        }

        void PrintState()
        {
            var arr = stream.ToArray();
            var buff = stream.GetBuffer();
            Console.WriteLine("Str: [" + stream.Length + "] - " + arr.ToHexString());
            Console.WriteLine("Buf: [" + stream.Capacity + "] - " + buff.ToHexString());
        }
    }
}
