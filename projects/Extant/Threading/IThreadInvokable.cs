using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Threading
{
    public interface IThreadInvokable
    {
        bool HasFinished { get; }
        bool Succeeded { get; }
        Exception ThrownException { get; }

        void Run();
    }
}
