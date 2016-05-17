using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Extant.Threading
{
    public interface IThreadJob<out TResult>
    {
        void Start();
        void Wait();
        void Abort();
        bool IsRunning { get; }
        TResult Result { get; }
    }

    public interface IThreadJob : IThreadJob<bool>
    { }

    /// <summary>
    /// Used an alternative for the standard Tasks. This will create a new thread to execute a vertain job.
    /// </summary>
    public class ThreadJob<TResult> : IThreadJob<TResult>
    {
        private Func<TResult> _func;
        private Thread _thread;
        private bool _hasBeenStarted = false;
        private bool _isRunning = false;
        private TResult _result;

        public ThreadJob(Func<TResult> func)
        {
            this._func = func;
            this._result = default(TResult);
            this._thread = new Thread(new ThreadStart(_Run));
        }

        public static ThreadJob<TResult> StartNew(Func<TResult> func)
        {
            ThreadJob<TResult> job = new ThreadJob<TResult>(func);
            job.Start();

            return job;
        }

        private void _Run()
        {
            try
            {
                _result = _func();
            }
            catch (ThreadAbortException)
            { }
            finally
            {
                _isRunning = false;
            }
        }

        public void Start()
        {
            if (_hasBeenStarted)
                throw new InvalidOperationException("ThreadJob has already been started.");

            _isRunning = true;
            _hasBeenStarted = true;
            _thread.Start();
        }

        public void Wait()
        {
            if (!_hasBeenStarted)
                throw new InvalidOperationException("ThreadJob has not been started.");

            _thread.Join();
        }

        public void Abort()
        {
            if (!_hasBeenStarted)
                throw new InvalidOperationException("ThreadJob has not been started.");

            if (_isRunning)
                _thread.Abort();
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public TResult Result
        {
            get
            {
                Wait();
                return _result;
            }
        }
    }

    public class ThreadJob : ThreadJob<bool>
    {
        public ThreadJob(Action action)
            : base(() => { action(); return true; })
        { }

        public static ThreadJob StartNew(Action action)
        {
            ThreadJob job = new ThreadJob(action);
            job.Start();

            return job;
        }
    }
}
