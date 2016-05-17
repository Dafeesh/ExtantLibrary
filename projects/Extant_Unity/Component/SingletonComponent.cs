using UnityEngine;
using System;
using System.Collections.Generic;

using Extant.Event;

namespace Extant.Unity.Component
{

    public class SingletonComponent<T> : ComponentBase where T : ComponentBase
    {
        protected static WeakReference<T> _instance;

        public static T Global
        {
            get
            {
                if (_instance != null && !_instance.IsAlive)
                    _instance = null;

                if (_instance == null)
                {
                    var instances = (T[])FindObjectsOfType(typeof(T));
                    if (instances.Length == 0)
                    {
                        Debug.LogError("Signleton not found. An instance of " + typeof(T) +
                           " is needed in the scene, but there is none.");
                        return null;
                    }
                    else if (instances.Length >= 2)
                    {
                        Debug.LogError("Multiple singletons of type " + typeof(T) +
                           " found. Only one instance can exist in a scene at one time.");
                    }

                    _instance = new WeakReference<T>(instances[0]);
                }

                return _instance.TypedTarget;
            }
        }

        protected new void Awake()
        {

        }
    }
}
