using System;
using System.Collections.Generic;
using System.Linq;

using Extant.Threading;

namespace ExtantManualDebugging
{
    public class ThreadRunTest
    {
        class TestThreadRun : ThreadRun
        {
            public int SomeNumber = 0;

            public TestThreadRun()
                : base("ExampleName")
            {
                RegisterTickCall(Tick1, TimeSpan.FromMilliseconds(1000));
                RegisterTickCall(Tick2, TimeSpan.FromMilliseconds(500));
                RegisterTickCall(Tick3, TimeSpan.FromMilliseconds(333));
                RegisterTickCall(Tick4, TimeSpan.FromMilliseconds(250));
            }

            protected override void OnBegin()
            { }
            protected override void OnFinish(Exception unhandledException = null)
            { }

            private void Tick1()
            {
                Console.WriteLine("1");
            }

            private void Tick2()
            {
                Console.WriteLine(" 2");
            }

            private void Tick3()
            {
                Console.WriteLine("  3");
            }

            private void Tick4()
            {
                Console.WriteLine("   4");
            }
        }

        public void Run()
        {
            var thr = new TestThreadRun();
            thr.Start();

            Console.ReadKey();
            thr.Stop();
        }
    }
}
