using UnityEngine;
using System.Collections;

using Extant.Logging;
using Extant.Event;

namespace Extant.Unity.Component
{
    public abstract class ComponentBase : MonoBehaviour, IDebugLogging, IListeningForEvents
    {
        [System.Flags]
        public enum DebugPostingScope
        {
            None = 0,
            Messages = 1,
            Warnings = 2,
            Errors = 4
        }

        private DebugLogger _log = null;
        private DebugPostingScope _logPostScope = DebugPostingScope.None;

        /// <summary>
        /// Called once one frame after the component is created. Called before OnEnable() and Start().
        /// </summary>
        protected virtual void OnInitialize() { }
        protected void Awake()
        {
            this._log = new DebugLogger(this.name + ":" + this.GetType().Name);
            DebugPosting = DebugPostingScope.Warnings | DebugPostingScope.Errors;
            OnInitialize();
        }

        /// <summary>
        /// Called whenever this component is being destroyed.
        /// Called after OnDisable().
        /// </summary>
        protected virtual void OnTerminate() { }
        protected void OnDestroy()
        {
            OnTerminate();
            DebugPosting = DebugPostingScope.None;
        }

        protected void LocalAssert(bool test, string ifFailed, bool disableIfFailed = false)
        {
            if (!test)
            {
                Log.LogError("[ASSERT]: " + ifFailed);
                if (disableIfFailed)
                    this.enabled = false;
            }
        }

        protected void StartContinuousCoroutine(System.Action func, float delay = 0f)
        {
            StartCoroutine(RunContinuousCoroutine(func, delay));
        }

        private IEnumerator RunContinuousCoroutine(System.Action func, float delay)
        {
            WaitForSeconds wait = new WaitForSeconds(delay > 0f ? delay : 0f);
            while (true)
            {
                yield return wait;
                func.Invoke();
            }
        }

        protected void Invoke(System.Action func, float runAfterDelay)
        {
            StartCoroutine(RunActionAfter(func, runAfterDelay));
        }

        private IEnumerator RunActionAfter(System.Action func, float runAfterDelay)
        {
            yield return new WaitForSeconds(runAfterDelay);
            func.Invoke();
            yield return null;
        }

        ////////////////////

        public bool CanReceiveEvents
        {
            get
            {
                return this.enabled;
            }
        }

        public IDebugLogger Log
        {
            get
            {
                return _log;
            }
        }

        protected DebugPostingScope DebugPosting
        {
            get
            {
                return _logPostScope;
            }
            set
            {
                //Clear
                if ((_logPostScope & DebugPostingScope.Messages) == DebugPostingScope.Messages)
                    Log.MessageLogged -= Debug.Log;
                if ((_logPostScope & DebugPostingScope.Warnings) == DebugPostingScope.Warnings)
                    Log.WarningLogged -= Debug.LogWarning;
                if ((_logPostScope & DebugPostingScope.Errors) == DebugPostingScope.Errors)
                    Log.ErrorLogged -= Debug.LogError;

                _logPostScope = value;

                //Set
                if ((_logPostScope & DebugPostingScope.Messages) == DebugPostingScope.Messages)
                    Log.MessageLogged += Debug.Log;
                if ((_logPostScope & DebugPostingScope.Warnings) == DebugPostingScope.Warnings)
                    Log.WarningLogged += Debug.LogWarning;
                if ((_logPostScope & DebugPostingScope.Errors) == DebugPostingScope.Errors)
                    Log.ErrorLogged += Debug.LogError;

            }
        }
    }
}
