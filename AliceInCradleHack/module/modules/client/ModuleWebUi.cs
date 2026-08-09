using AliceInCradleHack.config;
using AliceInCradleHack.module.modules.client.webui;
using AliceInCradleHack.utils.client;
using System;
using System.Diagnostics;

namespace AliceInCradleHack.module.modules.client
{
    public class ModuleWebUi : Module
    {
        public const string ModuleName = "WebUI";

        public ModuleWebUi() : base(ModuleName, "Serves a browser-based control panel for modules and settings.", "Client")
        {
        }

        public readonly RangedValue<int> Port = new(23333, 1, 65535, "", "HTTP port of the WebUI server. Re-toggle the module to apply.");

        public readonly Value<bool> AutoOpenBrowser = new(true, "Open the WebUI page in the default browser on enable.");

        private readonly WebUiServer _server = new WebUiServer();

        public override void Initialize()
        {
        }

        public override void Enable()
        {
            int port = Port;
            try
            {
                _server.Start(port);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start WebUI on port {port}", ex);
                Log.Error($"Try running the game as administrator, or run: netsh http add urlacl url=http://127.0.0.1:{port}/ user=%USERNAME%");
                throw;
            }

            string url = $"http://127.0.0.1:{port}/";
            Log.Info($"WebUI started: {url}");

            if (AutoOpenBrowser)
            {
                try { Process.Start(url); }
                catch (Exception ex) { Log.Error("Failed to open browser", ex); }
            }
        }

        public override void Disable()
        {
            _server.Stop();
            Log.Info("WebUI stopped.");
        }
    }
}
