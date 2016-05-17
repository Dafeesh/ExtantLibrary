using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extant.Threading
{
    public class ThreadInvokeAction : ThreadInvokeFunc<bool>
    {
        public ThreadInvokeAction(Action a)
            : base(() => { a(); return true; })
        { }
    }
}
