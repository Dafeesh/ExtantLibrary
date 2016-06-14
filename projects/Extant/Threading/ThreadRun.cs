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
using Extant.Extensions;

namespace Extant.Threading
{
    //Base class used as a framework for classes that will run on its own thread.
    public abstract class ThreadRun
    {
        private static readonly NotSupportedException InvokeNotSupportedException
            = new NotSupportedException("This object is marked as not invokable.");

        private String _name;
        private Thread _thisThread;
        private bool _isInvokable;

        private List<TickCall> _tickCalls = new List<TickCall>();
        private Boolean _hasStarted = false;
        private Boolean _isStopping = false;
        private Boolean _isStopped = false;
        private Exception _unhandledException = null;

        private object _invokedLock = new object();
        private Queue<Action> _invokedActions = new Queue<Action>();
        private Queue<IThreadInvokable> _invokedFunctions = new Queue<IThreadInvokable>();

        /// <summary>
        /// Creates an object for a thread to own and run an inheritted class.
        /// </summary>
        /// <param name="threadName">Name of the thread.</param>
        /// <param name="tickCalls">An array of actions with how how often they should be called each second.</param>
        protected ThreadRun(String threadName, bool isInvokable = true)
        {
            if (string.IsNullOrEmpty(threadName))
                throw new ArgumentException("ThreadName cannot be null or empty.");

            this._name = threadName;
            this._thisThread = new Thread(new ThreadStart(Run)) { Name = "<" + threadName + ">" };
            this._isInvokable = isInvokable;
        }

        ~ThreadRun()
        {
            this.Stop(true);
        }

        protected abstract void OnBegin();
        protected abstract void OnFinish(Exception unhandledException = null);

        /// <summary>
        /// Used to register an action to be called so many times per second on this thread.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callsPerSecond"></param>
        protected void RegisterTickCall(Action action, TimeSpan timeBetweenCalls)
        {
            if (action == null)
                throw new ArgumentException("Action cannot be null.");

            _tickCalls.Add(new TickCall(action, timeBetweenCalls));
        }

        /// <summary>
        /// Main run function for this thread.
        /// </summary>
        private void Run()
        {
            try
            {
                OnBegin();

                long timeUntilNextTick = 0;
                while (!_isStopping)
                {
                    HandleTickCalls(out timeUntilNextTick);
                    if (_isInvokable)
                        HandleInvoked();

                    //Sleep until the next tick
                    Thread.Sleep((int)timeUntilNextTick);
                }
            }
            catch (Exception e)
            {
                _unhandledException = e;
                this.Stop();
            }

            OnFinish(_unhandledException);
            _isStopped = true;
        }

        /// <summary>
        /// Handles the timing and calls of all ticks.
        /// </summary>
        void HandleTickCalls(out long timeUntilNextTick)
        {
            timeUntilNextTick = int.MaxValue;
            foreach (var call in _tickCalls)
            {
                if (call.IsReady)
                {
                    call.ActionCall.Invoke();
                    call.RestartCallTimer();
                }

                if (call.RemainingMilliseconds < timeUntilNextTick)
                    timeUntilNextTick = call.RemainingMilliseconds;
            }
        }

        /// <summary>
        /// Call all invoked functions in the queue.
        /// </summary>
        private void HandleInvoked()
        {
            lock (_invokedLock)
            {
                while (_invokedFunctions.HasNext())
                {
                    _invokedFunctions.Dequeue().Run();
                    if (this.IsStopped)
                        break;
                }

                while (_invokedActions.HasNext())
                {
                    _invokedActions.Dequeue().Invoke();
                    if (this.IsStopped)
                        break;
                }
            }
        }

        /// <summary>
        /// Starts the thread controlling this object.
        /// </summary>
        public void Start()
        {
            if (this.HasStarted)
                throw new InvalidOperationException("This ThreadRun has already been started!");

            _thisThread.Start();
            _hasStarted = true;
        }

        /// <summary>
        /// Instructs the thread to stop.
        /// </summary>
        public void Stop(bool blockWhileStopping = false)
        {
            if (!_isStopping && !_isStopped)
            {
                _isStopping = true;

                if (blockWhileStopping && !CurrentlyRunningInThisThread)
                    while (!_isStopped)
                        Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Adds a task to be executed during this object's main thread.
        /// </summary>
        /// <param name="func">Task to be run on this object's main thread.</param>
        public void Invoke(IThreadInvokable func)
        {
            if (!_isInvokable)
                throw InvokeNotSupportedException;
            lock (_invokedLock)
            {
                _invokedFunctions.Enqueue(func);
            }
        }

        /// <summary>
        /// Adds a task to be executed during this object's main thread.
        /// </summary>
        /// <param name="action">Task to be run on this object's main thread.</param>
        public void Invoke(Action action)
        {
            if (!_isInvokable)
                throw InvokeNotSupportedException;
            lock (_invokedLock)
            {
                _invokedActions.Enqueue(action);
            }
        }

        protected bool CurrentlyRunningInThisThread
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId == _thisThread.ManagedThreadId;
            }
        }

        public bool IsInvokable
        {
            get
            {
                return _isInvokable;
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

        public Int32 ManagedThreadId
        {
            get
            {
                return _thisThread.ManagedThreadId;
            }
        }

        public Exception UnhandledException
        {
            get
            {
                return _unhandledException;
            }
        }

        internal class TickCall
        {
            public Action ActionCall { get; private set; }

            private TimeoutTimer _timer;

            public TickCall(Action action, TimeSpan timeBetweenCalls)
            {
                this.ActionCall = action;
                this._timer = TimeoutTimer.StartNew(timeBetweenCalls);
            }

            public void RestartCallTimer()
            {
                _timer.Restart();
            }

            public bool IsReady
            {
                get
                {
                    return _timer.IsTimedOut;
                }
            }

            public long RemainingMilliseconds
            {
                get
                {
                    return _timer.RemainingMilliseconds;
                }
            }
        }
    }
}
