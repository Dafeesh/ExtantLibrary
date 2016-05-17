using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extant.Event
{
    public interface ISharedEventSubscriber
    {
        bool IsReceivingSharedEvents { get; }
    }
}
