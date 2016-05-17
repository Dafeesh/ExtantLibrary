using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Extant.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Unsubscribes all Delegates referencing -obj- in static -type-'s list of static events.
        /// </summary>
        public static void ClearAllStaticEventReferencesTo(this Type type, object obj)
        {
            foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.Static | BindingFlags.Public))
            {
                FieldInfo fieldInfo = type.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.NonPublic);

                MulticastDelegate eventValue = (MulticastDelegate)fieldInfo.GetValue(null);
                if (eventValue != null) // will be null if no subscribed event consumers
                {
                    Delegate[] delegates = eventValue.GetInvocationList();
                    foreach (Delegate d in delegates)
                    {
                        if (d.Target == obj)
                        {
                            eventInfo.RemoveEventHandler(null, d);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets all static events in static class -type-.
        /// </summary>
        public static void ClearAllStaticEvents(this Type type)
        {
            foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.Static | BindingFlags.Public))
            {
                FieldInfo field = type.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.NonPublic);
                field.SetValue(null, null);
            }
        }
    }
}
