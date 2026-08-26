using AliceInCradleHack.utils.client;
using System;
using System.IO;
using System.Text;

namespace AliceInCradleHack.module.modules.client.webui
{
    /// <summary>
    /// Serves the embedded single-file WebUI page (resources/webui/index.html,
    /// dark theme, English, no external resources).
    /// </summary>
    public static class WebUiPage
    {
        private const string ResourceName = Client.ClientName + ".resources.webui.index.html";

        private static string _html;

        public static string Html => _html ??= LoadHtml();

        private static string LoadHtml()
        {
            try
            {
                var assembly = typeof(WebUiPage).Assembly;
                var stream = assembly.GetManifestResourceStream(ResourceName);
                if (stream == null)
                {
                    foreach (var name in assembly.GetManifestResourceNames())
                    {
                        if (name.EndsWith("index.html", StringComparison.OrdinalIgnoreCase))
                        {
                            stream = assembly.GetManifestResourceStream(name);
                            break;
                        }
                    }
                }

                if (stream == null)
                {
                    Log.Error("WebUI page resource not found in assembly");
                    return "<!DOCTYPE html><html><body><h1>WebUI page resource missing</h1></body></html>";
                }

                using (stream)
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load the embedded WebUI page", ex);
                return "<!DOCTYPE html><html><body><h1>Failed to load the WebUI page</h1></body></html>";
            }
        }
    }
}
