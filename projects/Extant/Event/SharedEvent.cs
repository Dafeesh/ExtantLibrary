using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Extant.Event
{
    public delegate void SharedEventHandler<TArgs>(object sender, TArgs args);

    public class SharedEvent<TArgs>
    {
        object _lock = new object();
        Dictionary<WeakReference<ISharedEventSubscriber>, SharedEventHandler<TArgs>> _subscribers = new Dictionary<WeakReference<ISharedEventSubscriber>, SharedEventHandler<TArgs>>();
        Dictionary<WeakReference<ISharedEventSubscriber>, SharedEventHandler<TArgs>> _subscribersLate = new Dictionary<WeakReference<ISharedEventSubscriber>, SharedEventHandler<TArgs>>();

        public void Raise(ISharedEventSubscriber sender, TArgs args)
        {
            lock (_lock)
            {
                //Trigger all subs, remove null reference object
                foreach (var toRemove in _subscribers.Where((kvp) =>
                {
                    if (kvp.Key.IsAlive)
                    {


                        if (kvp.Key.TypedTarget.IsReceivingSharedEvents)
                            kvp.Value(sender, args);
                        return false;
                    }
                    else
                        return true;
                }).ToArray())
                {
                    _subscribers.Remove(toRemove.Key);
                }

                //Trigger all subs flagged as late, remove null reference object
                foreach (var toRemove in _subscribersLate.Where((kvp) =>
                {
                    if (kvp.Key.IsAlive)
                    {
                        if (kvp.Key.TypedTarget.IsReceivingSharedEvents)
                            kvp.Value(sender, args);
                        return false;
                    }
                    else
                        return true;
                }).ToArray())
                {
                    _subscribers.Remove(toRemove.Key);
                }
            }
        }

        public void Subscribe(ISharedEventSubscriber subscriber, SharedEventHandler<TArgs> toCall, bool isLate = false)
        {
            if (Object.ReferenceEquals(subscriber, null) || Object.ReferenceEquals(toCall, null))
                throw new ArgumentException("Subscriber and toCall cannot be null.");

            lock (_lock)
            {
                if (isLate)
                    _subscribersLate.Add(new WeakReference<ISharedEventSubscriber>(subscriber), toCall);
                else
                    _subscribers.Add(new WeakReference<ISharedEventSubscriber>(subscriber), toCall);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
                _subscribersLate.Clear();
            }
        }
    }
}
