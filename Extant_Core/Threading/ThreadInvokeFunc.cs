using System;
using System.Threading;

namespace Extant.Threading
{
    /// <summary>
    /// Used as a way to pass a task to a different thread and track its progress.
    /// </summary>
    public class ThreadInvokeFunc<T> : IThreadInvokable
    {
        private Func<T> _task;
        private T _result;

        public bool HasFinished { get; private set; }
        public bool Succeeded { get; private set; }
        public Exception ThrownException { get; private set; }

        public ThreadInvokeFunc(Func<T> task)
        {
            this.HasFinished = false;
            this.Succeeded = false;
            this.ThrownException = null;

            this._task = task;
        }

        public void Run()
        {
            try
            {
                _result = _task();
                Succeeded = true;
            }
            catch (Exception e)
            {
                ThrownException = e;
                Succeeded = false;
            }
            finally
            {
                HasFinished = true;
            }
        }

        public T WaitForResult()
        {
            while (!HasFinished)
            {
                Thread.Sleep(0);
            }

            return _result;
        }
    }
}
