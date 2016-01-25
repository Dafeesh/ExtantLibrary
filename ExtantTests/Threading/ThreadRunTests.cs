using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Extant.Threading;

namespace ExtantTests.Threading
{
    [TestClass]
    public class ThreadRunTests
    {
        public class TestThreadRun : ThreadRun
        {
            public int SomeNumber = 0;

            public TestThreadRun()
                : base("ExampleName")
            {
                this.RegisterTickCall(Tick, ThreadRun.UncappedTicksPerSecond);
            }

            private void Tick()
            {
                SomeNumber++;
            }
        }

        [TestMethod]
        public void ThreadRun_InvokingSupported()
        {
            TestThreadRun thr = new TestThreadRun();
            thr.Start();

            ThreadInvokeAction invokeAction = new ThreadInvokeAction(() => { Console.WriteLine("Invoked action called."); });
            thr.Invoke(invokeAction);

            Assert.AreEqual(true, invokeAction.WaitForResult(), "Failed to run invoke action.");
            Assert.AreNotEqual(0, thr.SomeNumber, "OnTick was not called before first invoke.");

            thr.Stop(new ThreadStopResult(ThreadStopType.Success, ThreadStopSource.Parent), true);

            Assert.AreEqual(thr.IsStopped, true, "Thread did not stop.");
            Assert.AreEqual(thr.StopResult.Source, ThreadStopSource.Parent, "Different StopReason value after stopping.");
        }
    }
}
