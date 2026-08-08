using System;
using System.Net;
using System.Threading;

namespace AliceInCradleHack.module.modules.client.webui
{
    /// <summary>
    /// Embedded HTTP server based on HttpListener. Serves the WebUI page and REST API.
    /// </summary>
    public class WebUiServer
    {
        private HttpListener _listener;
        private Thread _listenThread;
        private volatile bool _running;

        public int Port { get; private set; }

        public bool IsRunning => _running && _listener != null && _listener.IsListening;

        public void Start(int port)
        {
            Stop();

            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Start();

            _running = true;
            _listenThread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "WebUiServer"
            };
            _listenThread.Start();
        }

        public void Stop()
        {
            _running = false;

            var listener = _listener;
            _listener = null;
            if (listener != null)
            {
                try { listener.Stop(); } catch { }
                try { listener.Close(); } catch { }
            }

            var thread = _listenThread;
            _listenThread = null;
            if (thread != null && thread.IsAlive && Thread.CurrentThread != thread)
            {
                try { thread.Join(500); } catch { }
            }
        }

        private void ListenLoop()
        {
            while (_running)
            {
                HttpListenerContext context;
                try
                {
                    var listener = _listener;
                    if (listener == null || !listener.IsListening) break;
                    context = listener.GetContext();
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (InvalidOperationException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebUI accept error: {ex.Message}");
                    continue;
                }

                ThreadPool.QueueUserWorkItem(_ => HandleSafe(context));
            }
        }

        private void HandleSafe(HttpListenerContext context)
        {
            try
            {
                WebUiApi.HandleRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebUI request error: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }
    }
}
