using System;
using System.Threading;

namespace Extant
{
    public class ThreadTaskParams
    {
        private object[] values;

        public ThreadTaskParams(object[] parameters)
        {
            values = parameters;
        }

        public object this[int i]
        {
            get
            {
                return values[i];
            }
        }
    }

    public class ThreadTask<TReturn>
    {
        private bool hasStarted;
        private bool isRunning;
        private Func<ThreadTaskParams, TReturn> task;
        private ThreadTaskParams taskParams;
        private TReturn result;

        public Exception ThrownException 
        { get; set; }

        private Thread thread;

        public ThreadTask(Func<ThreadTaskParams, TReturn> task, object[] taskParams)
        {
            this.hasStarted = false;
            this.isRunning = false;
            this.task = task;
            this.taskParams = new ThreadTaskParams(taskParams);
        }

        public void Start()
        {
            if (hasStarted)
                throw new InvalidOperationException("ThreadTask has already been started!");
            hasStarted = true;
            isRunning = true;
            thread = new Thread(new ThreadStart(_Run));
            thread.Start();
        }

        private void _Run()
        {
            try
            {
                result = task(taskParams);
            }
            catch (Exception e)
            {
                ThrownException = e;
            }
            finally
            {
                isRunning = false;
            }
        }

        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        public TReturn Result
        {
            get
            {
                while (IsRunning)
                { }

                return result;
            }
        }
    }
}
