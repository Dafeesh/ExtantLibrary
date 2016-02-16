/* ThreadRun.cs
 * Author: Blake Scherschel
 * Last Update: 2015/2/3
 * 
 * Purpose: Class used as a means of cleanly running a process on
 * a separate thread wrapped into one object.
 * 
 *   The author just needs to inherit ThreadRun and override two functions:
 * Begin() -> Called when the object is instructed to begin running.
 * Finish() -> Called when the thread has ended either by an exception or by instruction.
 * These functions all operate on the same thread so they are thread safe.
 *   Then the auther will need to register Tick methods to be called.
 *   Do this by calling RegisterTickCall() in the constructor to setup however
 * many actions need to be called by any variable amount of times per second.
 * 
 * Notes:
 * -Avoid intensive processes in the constructor. Instead, use Begin.
 * -For functions that are not thread safe, use the public Invoke function.
 * -After instantiation of this class, use Start and Stop to dictate when it executes.
 * -If an object throws an UnhandledException then the Exception is stored in the public 
 *  UnhandledException and the thread is stopped.
 */

using System;
using System.Collections.Generic;
using System.Threading;

using Extant.Util;

namespace Extant.Threading
{
    //Base class used as a framework for classes that will run on a separate
    //thread. When inheritted, the inheritting class must override a RunLoop method
    //that will be called until requested to stop.
    public abstract class ThreadRun
    {
        public const Int32 UncappedTicksPerSecond = int.MaxValue;

        private String _name;
        private LockValuePair<Thread> _thisThread;

        private Boolean _hasStarted = false;
        private Boolean _isStopped = false;
        private ThreadStopResult _stopResult = null;
        private Exception _unhandledException = null;

        private Dictionary<Action, TimeoutTimer> _tickTimers = new Dictionary<Action, TimeoutTimer>();
        private LockValuePair<Queue<IThreadInvokable>> _invokedFunctions = new LockValuePair<Queue<IThreadInvokable>>(new Queue<IThreadInvokable>());

        /// <summary>
        /// Creates an object for a thread to own and run an inheritted class.
        /// </summary>
        /// <param name="threadName">Name of the thread.</param>
        /// <param name="actionTicksPerSecondPairs">An action and how many times to call each second.</param>
        protected ThreadRun(String threadName)
        {
            if (string.IsNullOrEmpty(threadName))
                throw new ArgumentException("ThreadName cannot be null or empty.");

            this._name = threadName;
            this._thisThread = new LockValuePair<Thread>(
                new Thread(new ThreadStart(Run))
                { Name = "<" + threadName + ">" }
            );
        }

        ~ThreadRun()
        {
            this.Stop(new ThreadStopResult(ThreadStopType.Success, ThreadStopSource.GarbageCollection), true);
        }

        /// <summary>
        /// Main run function for this thread.
        /// </summary>
        private void Run()
        {
            OnBegin();

            ToggleTickTimers(true);
            long timeUntilNextTick = 0;

            while (!IsStopped)
            {
                try
                {
                    HandleTicks(out timeUntilNextTick);
                    if (this.IsStopped)
                        break;

                    HandleInvokedFunctions();
                    if (this.IsStopped)
                        break;

                    //Sleep until the next tick
                    Thread.Sleep((int)timeUntilNextTick);
                }
                catch (Exception e)
                {
                    this._unhandledException = e;
                    this.Stop(new ThreadStopResult(ThreadStopType.UnhandledException, ThreadStopSource.Self));
                }
            }

            ToggleTickTimers(false);

            OnFinish(_unhandledException == null);
        }

        /// <summary>
        /// Handles the timing and calls of all ticks.
        /// </summary>
        /// Returns false if the thread has been stopped during tick calls.
        /// <returns></returns>
        void HandleTicks(out long timeUntilNextTick)
        {
            timeUntilNextTick = int.MaxValue;
            foreach (var tickTimer in _tickTimers)
            {
                if (tickTimer.Value.IsTimedOut)
                {
                    lock (_thisThread.Lock)
                    {
                        tickTimer.Key();
                    }
                    tickTimer.Value.Restart();

                    if (this.IsStopped)
                        return;
                }

                if (timeUntilNextTick > tickTimer.Value.RemainingMilliseconds)
                    timeUntilNextTick = tickTimer.Value.RemainingMilliseconds;
            }
        }

        /// <summary>
        /// Call all invoked functions in the queue.
        /// </summary>
        private void HandleInvokedFunctions()
        {
            lock (_invokedFunctions.Lock)
            {
                IThreadInvokable invokedFunction;
                while (_invokedFunctions.Value.Count > 0)
                {
                    invokedFunction = _invokedFunctions.Value.Dequeue();
                    invokedFunction.Run();
                    if (this.IsStopped)
                        break;
                }
            }
        }

        /// <summary>
        /// Toggles if timers should be running or not.
        /// </summary>
        private void ToggleTickTimers(bool toggleRunning)
        {
            foreach (var tickTimer in _tickTimers)
            {
                if (tickTimer.Value.IsRunning != toggleRunning)
                    if (toggleRunning)
                        tickTimer.Value.Start();
                    else
                        tickTimer.Value.Stop();
            }
        }

        /// <summary>
        /// Adds a tick function to be called so many times every second.
        /// </summary>
        protected void RegisterTickCall(Action tickFunction, int callsPerSecond)
        {
            if (tickFunction == null)
                throw new ArgumentException("Tick function cannot be null.");
            if (callsPerSecond <= 0)
                throw new ArgumentException("Ticks per second must be greater than zero or equal to 'UncappedTicksPerSecond'.");

            _tickTimers.Add(tickFunction,
                new TimeoutTimer(TimeSpan.FromMilliseconds(1000 / callsPerSecond)));
        }

        #region Object events

        /// <summary>
        /// Called when thread instructed to start.
        /// Called before any tick.
        /// </summary>
        protected virtual void OnBegin() { }

        /// <summary>
        /// Called when thread is finished, fails via instruction, or unhandled exception.
        /// Called after all ticks.
        /// </summary>
        /// <param name="finishedWithoutError">The thread finished properly.</param>
        protected virtual void OnFinish(bool finishedWithoutError) { }

        #endregion Object events

        /// <summary>
        /// Starts the thread controlling this object.
        /// </summary>
        public void Start()
        {
            if (HasStarted)
                throw new InvalidOperationException("This ThreadRun has already been started!");

            _thisThread.Value.Start();
            _hasStarted = true;
        }

        /// <summary>
        /// Instructs the thread to stop.
        /// </summary>
        public void Stop(ThreadStopResult reason, bool blockWhileStopping = false)
        {
            if (!IsStopped)
            {
                _isStopped = true;
                _stopResult = reason;

                if (blockWhileStopping)
                {
                    if (_thisThread.Value.IsAlive && Thread.CurrentThread.ManagedThreadId != _thisThread.Value.ManagedThreadId)
                        _thisThread.Value.Join();
                }
            }
        }

        /// <summary>
        /// Adds a Task to be executed during this object's main thread.
        /// </summary>
        /// <param name="func">ThreadTask to be run on this object's main thread.</param>
        public void Invoke(IThreadInvokable func)
        {
            lock (_invokedFunctions.Lock)
            {
                _invokedFunctions.Value.Enqueue(func);
            }
        }

        public bool HasStarted
        {
            get
            {
                return _hasStarted;
            }
        }

        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }
        }

        public ThreadStopResult StopResult
        {
            get
            {
                return _stopResult;
            }
        }

        public Int32 ManagedThreadId
        {
            get
            {
                return _thisThread.Value.ManagedThreadId;
            }
        }
    }
}
