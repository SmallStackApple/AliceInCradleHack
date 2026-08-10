using AliceInCradleHack.utils.client;
using System;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class GuiBehaviour : MonoBehaviour
    {
        private static bool _created;
        private static bool _renderFailed;

        public static void EnsureCreated()
        {
            if (_created) return;
            _created = true;
            try
            {
                var host = new GameObject("AliceInCradleHack.Gui");
                UnityEngine.Object.DontDestroyOnLoad(host);
                host.AddComponent<GuiBehaviour>();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to create GUI host object", ex);
            }
        }

        private void OnGUI()
        {
            if (_renderFailed) return;
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            try
            {
                DynamicIsland.Instance.Render();
            }
            catch (Exception ex)
            {
                _renderFailed = true;
                Log.Error("Dynamic island render failed", ex);
            }
        }
    }
}
