using System;
using UnityEngine;

namespace AliceInCradleHack.utils.client
{
    /// <summary>
    /// Hosts an OnGUI renderer on a persistent hidden GameObject. The static state lives
    /// on the closed generic type, so each derived class gets its own independent host.
    /// </summary>
    public abstract class OnGuiRenderer<T> : MonoBehaviour where T : OnGuiRenderer<T>
    {
        private static bool _created;
        private static bool _renderFailed;

        public static T Instance { get; private set; }

        /// <summary>Creates the host GameObject once. Safe to call every frame.</summary>
        public static void EnsureCreated(string hostName)
        {
            if (_created) return;
            _created = true;
            try
            {
                var host = new GameObject(hostName);
                UnityEngine.Object.DontDestroyOnLoad(host);
                Instance = host.AddComponent<T>();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create GUI host object '{hostName}'", ex);
            }
        }

        protected virtual void OnGUI()
        {
            if (_renderFailed) return;
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            try
            {
                Render();
            }
            catch (Exception ex)
            {
                _renderFailed = true;
                Log.Error($"{GetType().Name} render failed", ex);
            }
        }

        protected abstract void Render();
    }
}
