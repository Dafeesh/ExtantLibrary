using System;
using System.Collections.Generic;
using System.Linq;

using Extant.Logging;
using Extant.Util;

namespace ExtantManualDebugging
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleTimer runTimer = SimpleTimer.StartNew();

            DebugLogger log = new DebugLogger("$");
            log.Logged += Console.WriteLine;

            (new NetTest(log))
            //
            .Run();

            Console.WriteLine("\nDone. [" + runTimer.Elapsed.ToString() + "]");
            Console.ReadKey();
        }
    }
}
