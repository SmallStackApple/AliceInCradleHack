using AliceInCradleHack.config;
using AliceInCradleHack.config.group;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AliceInCradleHack.module.modules.client.webui
{
    /// <summary>
    /// Routes HTTP requests to the WebUI page and the REST API backed by ModuleManager.
    /// </summary>
    public static class WebUiApi
    {
        private const string SelfModuleName = ModuleWebUi.ModuleName;

        public static void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            string path = request.Url.AbsolutePath.Trim('/');
            string[] segments = path.Length == 0
                ? Array.Empty<string>()
                : path.Split('/').Select(Uri.UnescapeDataString).ToArray();

            if (request.HttpMethod == "GET" && segments.Length == 0)
            {
                WriteText(context, WebUiPage.Html, "text/html; charset=utf-8");
                return;
            }

            if (segments.Length >= 1 && segments[0] == "api")
            {
                HandleApiRequest(context, segments);
                return;
            }

            WriteError(context, 404, "Not found");
        }

        private static void HandleApiRequest(HttpListenerContext context, string[] segments)
        {
            var manager = ModuleManager.Instance;

            // GET /api/modules
            if (context.Request.HttpMethod == "GET" && segments.Length == 2 && segments[1] == "modules")
            {
                var modules = manager.GetAllModules()
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .Select(m => new
                    {
                        name = m.Name,
                        description = m.Description,
                        category = m.Category,
                        isEnabled = m.IsEnabled,
                        isSelf = m.Name == SelfModuleName
                    });
                WriteJson(context, modules);
                return;
            }

            // /api/modules/{name}/...
            if (segments.Length >= 3 && segments[1] == "modules")
            {
                string moduleName = segments[2];
                var module = manager.GetModuleByName(moduleName);
                if (module == null)
                {
                    WriteError(context, 404, $"Module not found: {moduleName}");
                    return;
                }

                // POST /api/modules/{name}/toggle
                if (context.Request.HttpMethod == "POST" && segments.Length == 4 && segments[3] == "toggle")
                {
                    if (moduleName == SelfModuleName && module.IsEnabled)
                    {
                        WriteError(context, 400, "Cannot disable WebUI itself from the web page. Use the console command: module toggle WebUI");
                        return;
                    }
                    manager.ToggleModule(moduleName);
                    WriteJson(context, new { name = moduleName, isEnabled = module.IsEnabled });
                    return;
                }

                // GET /api/modules/{name}/settings
                if (context.Request.HttpMethod == "GET" && segments.Length == 4 && segments[3] == "settings")
                {
                    var settings = module.Settings.GetAllLeafNodes().Select(n =>
                    {
                        object value = n.GetValueObject();
                        if (n.Type == AliceInCradleHack.config.ValueType.EnumChoice && value != null)
                            value = value.ToString();
                        var result = new JObject
                        {
                            ["path"] = n.GetPath(),
                            ["name"] = n.Name,
                            ["description"] = n.Description,
                            ["type"] = n.Type.ToString(),
                            ["value"] = value == null ? JValue.CreateNull() : JToken.FromObject(value),
                            ["isEditable"] = IsNodeEditable(n)
                        };
                        if (n is IRangedValue ranged)
                        {
                            result["min"] = ranged.MinObject == null ? JValue.CreateNull() : JToken.FromObject(ranged.MinObject);
                            result["max"] = ranged.MaxObject == null ? JValue.CreateNull() : JToken.FromObject(ranged.MaxObject);
                            result["suffix"] = ranged.Suffix ?? "";
                        }
                        if (n.Type == AliceInCradleHack.config.ValueType.EnumChoice || n.Type == AliceInCradleHack.config.ValueType.MultiChoice)
                        {
                            var choices = GetEnumChoices(n);
                            if (choices != null) result["choices"] = choices;
                        }
                        else if (n is StringListValue list)
                        {
                            result["choices"] = new JArray(list.Items);
                        }
                        return result;
                    });
                    WriteJson(context, settings);
                    return;
                }

                // POST /api/modules/{name}/settings  body: {"path": "...", "value": ...}
                if (context.Request.HttpMethod == "POST" && segments.Length == 4 && segments[3] == "settings")
                {
                    var payload = ParseBody(context);
                    if (payload == null) return;

                    string settingPath = payload["path"]?.ToString();
                    if (string.IsNullOrWhiteSpace(settingPath))
                    {
                        WriteError(context, 400, "Missing 'path' field");
                        return;
                    }

                    if (!IsNodeEditable(manager.GetSettingNode(moduleName, settingPath)))
                    {
                        WriteError(context, 400, "This setting is read-only");
                        return;
                    }

                    object value = UnwrapToken(payload["value"]);
                    if (!manager.SetSettingValue(moduleName, settingPath, value))
                    {
                        WriteError(context, 400, "Failed to apply setting. Check the value type.");
                        return;
                    }

                    WriteJson(context, new { path = settingPath, value = manager.GetSettingValue(moduleName, settingPath) });
                    return;
                }
            }

            // /api/config/...
            if (segments.Length >= 2 && segments[1] == "config")
            {
                // GET /api/config/export  -> download a single merged JSON
                if (context.Request.HttpMethod == "GET" && segments.Length == 3 && segments[2] == "export")
                {
                    WriteDownload(context, ConfigSystem.ExportAllToJson(), "aic-hack-config.json");
                    return;
                }

                // POST /api/config/import  body: merged JSON
                if (context.Request.HttpMethod == "POST" && segments.Length == 3 && segments[2] == "import")
                {
                    string body = ReadBody(context);
                    if (!ConfigSystem.ImportAllFromJson(body))
                    {
                        WriteError(context, 400, "Import failed: invalid or incompatible JSON");
                        return;
                    }
                    manager.ReapplyEnabledStates();
                    WriteJson(context, new { ok = true, message = "Config imported." });
                    return;
                }

                // GET /api/config/files  -> list saved single-file configs
                if (context.Request.HttpMethod == "GET" && segments.Length == 3 && segments[2] == "files")
                {
                    WriteJson(context, ConfigSystem.ListSavedFiles());
                    return;
                }

                // POST /api/config/save  body: {"name": "..."}
                if (context.Request.HttpMethod == "POST" && segments.Length == 3 && segments[2] == "save")
                {
                    var payload = ParseBody(context);
                    if (payload == null) return;
                    string name = payload["name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        WriteError(context, 400, "Missing 'name' field");
                        return;
                    }
                    string saved = ConfigSystem.SaveAllToFile(name);
                    if (saved == null)
                    {
                        WriteError(context, 400, "Failed to save config");
                        return;
                    }
                    WriteJson(context, new { ok = true, name = saved, message = $"Config saved as '{saved}'." });
                    return;
                }

                // POST /api/config/load  body: {"name": "..."}
                if (context.Request.HttpMethod == "POST" && segments.Length == 3 && segments[2] == "load")
                {
                    var payload = ParseBody(context);
                    if (payload == null) return;
                    string name = payload["name"]?.ToString();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        WriteError(context, 400, "Missing 'name' field");
                        return;
                    }
                    if (!ConfigSystem.LoadAllFromFile(name))
                    {
                        WriteError(context, 400, "Failed to load config");
                        return;
                    }
                    manager.ReapplyEnabledStates();
                    WriteJson(context, new { ok = true, name = name, message = $"Config '{name}' loaded." });
                    return;
                }
            }

            WriteError(context, 404, "Not found");
        }

        private static JObject ParseBody(HttpListenerContext context)
        {
            string body = ReadBody(context);
            try
            {
                return JObject.Parse(body);
            }
            catch (Exception)
            {
                WriteError(context, 400, "Request body is not valid JSON");
                return null;
            }
        }

        private static bool IsNodeEditable(Value node)
        {
            return node != null && node is not ValueGroup && node.IsEditable;
        }

        private static JArray GetEnumChoices(Value node)
        {
            try
            {
                var choices = node.GetType().GetProperty("Choices")?.GetValue(node) as System.Collections.IEnumerable;
                if (choices == null) return null;
                var array = new JArray();
                foreach (var choice in choices)
                    array.Add(choice?.ToString());
                return array;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static object UnwrapToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            if (token is JArray array)
                return array.Values<string>().ToArray();
            return token is JValue jv ? jv.Value : token.ToString();
        }

        private static string ReadBody(HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                return reader.ReadToEnd();
        }

        private static void WriteJson(HttpListenerContext context, object data, int statusCode = 200)
        {
            WriteText(context, JsonConvert.SerializeObject(data), "application/json; charset=utf-8", statusCode);
        }

        private static void WriteError(HttpListenerContext context, int statusCode, string message)
        {
            WriteJson(context, new { error = message }, statusCode);
        }

        private static void WriteDownload(HttpListenerContext context, string content, string fileName)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private static void WriteText(HttpListenerContext context, string content, string contentType, int statusCode = 200)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
    }
}
