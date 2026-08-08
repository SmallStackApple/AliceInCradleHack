using AliceInCradleHack.config;
using AliceInCradleHack.module.modules.client.webui;
using System;
using System.Diagnostics;

namespace AliceInCradleHack.module.modules.client
{
    public class ModuleWebUi : Module
    {
        public const string ModuleName = "WebUI";

        public override string Name => ModuleName;
        public override string Description => "Serves a browser-based control panel for modules and settings.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Client";

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
                Console.WriteLine($"Failed to start WebUI on port {port}: {ex.Message}");
                Console.WriteLine($"Try running the game as administrator, or run: netsh http add urlacl url=http://127.0.0.1:{port}/ user=%USERNAME%");
                throw;
            }

            string url = $"http://127.0.0.1:{port}/";
            Console.WriteLine($"WebUI started: {url}");

            if (AutoOpenBrowser)
            {
                try { Process.Start(url); }
                catch (Exception ex) { Console.WriteLine($"Failed to open browser: {ex.Message}"); }
            }
        }

        public override void Disable()
        {
            _server.Stop();
            Console.WriteLine("WebUI stopped.");
        }
    }
}
