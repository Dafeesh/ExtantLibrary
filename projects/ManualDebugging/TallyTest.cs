using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Extant.Util;

namespace ExtantManualDebugging
{
    class TallyTest
    {
        public void Run()
        {
            LongTally tally = new LongTally();
            ConsoleKey key = ConsoleKey.A;
            while (key != ConsoleKey.Escape)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey();
                    tally.Add(1);
                }

                Thread.Sleep(5);
                
                Console.SetCursorPosition(0, 0);
                Console.Write(tally.Total + " @ " + tally.PerSecond);
            }
            Console.WriteLine("#");
        }
    }
}
