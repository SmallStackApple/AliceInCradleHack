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
                    WriteError(context, 404, $"模块不存在: {moduleName}");
                    return;
                }

                // POST /api/modules/{name}/toggle
                if (context.Request.HttpMethod == "POST" && segments.Length == 4 && segments[3] == "toggle")
                {
                    if (moduleName == SelfModuleName && module.IsEnabled)
                    {
                        WriteError(context, 400, "不能从网页关闭 WebUI 自身，请在控制台执行 module toggle WebUI");
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
                        return new
                        {
                            path = n.GetPath(),
                            name = n.Name,
                            description = n.Description,
                            type = value?.GetType().Name ?? "String",
                            value,
                            isEditable = IsNodeEditable(n)
                        };
                    });
                    WriteJson(context, settings);
                    return;
                }

                // POST /api/modules/{name}/settings  body: {"path": "...", "value": ...}
                if (context.Request.HttpMethod == "POST" && segments.Length == 4 && segments[3] == "settings")
                {
                    var body = ReadBody(context);
                    JObject payload;
                    try
                    {
                        payload = JObject.Parse(body);
                    }
                    catch (Exception)
                    {
                        WriteError(context, 400, "请求体不是合法的 JSON");
                        return;
                    }

                    string settingPath = payload["path"]?.ToString();
                    if (string.IsNullOrWhiteSpace(settingPath))
                    {
                        WriteError(context, 400, "缺少 path 字段");
                        return;
                    }

                    if (!IsNodeEditable(manager.GetSettingNode(moduleName, settingPath)))
                    {
                        WriteError(context, 400, "该设置为只读");
                        return;
                    }

                    object value = UnwrapToken(payload["value"]);
                    if (!manager.SetSettingValue(moduleName, settingPath, value))
                    {
                        WriteError(context, 400, "设置失败，请检查值类型是否正确");
                        return;
                    }

                    WriteJson(context, new { path = settingPath, value = manager.GetSettingValue(moduleName, settingPath) });
                    return;
                }
            }

            WriteError(context, 404, "Not found");
        }

        private static bool IsNodeEditable(Value node)
        {
            return node != null && node is not ValueGroup && node.IsEditable;
        }

        private static object UnwrapToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
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
