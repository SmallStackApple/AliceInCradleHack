using AliceInCradleHack.utils.animation;
using AliceInCradleHack.utils.client;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AliceInCradleHack.module.modules.client.island
{
    /// <summary>
    /// Top-centered "dynamic island" overlay, inspired by openzen's DynamicIsland.
    /// The whole topmost click-through window IS the island: an opaque black capsule
    /// (WebView2 cannot be transparent on WinForms) clipped by a round-rect region,
    /// sized by spring animations and tracked to the game window each tick.
    /// </summary>
    public class DynamicIslandController
    {
        private readonly ManualResetEventSlim _ready = new(false);
        private Thread _uiThread;
        private IslandForm _form;

        public void Start()
        {
            if (_uiThread != null) return;
            _uiThread = new Thread(UiMain) { IsBackground = true, Name = "DynamicIslandUI" };
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();
            _ready.Wait(TimeSpan.FromSeconds(10));
        }

        public void Show() => Invoke(f => f.ShowIsland());
        public void Hide() => Invoke(f => f.HideIsland());

        private void Invoke(Action<IslandForm> action)
        {
            var form = _form;
            if (form == null || form.IsDisposed || !form.IsHandleCreated) return;
            try { form.BeginInvoke(action, form); } catch { }
        }

        private void UiMain()
        {
            try
            {
                _form = new IslandForm();
                _form.HandleCreated += (s, e) => _ready.Set();
                Application.Run(_form);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Gui] Dynamic island UI thread failed: " + ex.Message);
                _ready.Set();
            }
        }

        private sealed class IslandForm : Form
        {
            private const int TargetWidth = 300;
            private const int TargetHeight = 32;
            private const int TopMargin = 12;

            private readonly SpringAnimation _widthAnim = new(300f, 1.2f, 20f, 0f);
            private readonly SpringAnimation _heightAnim = new(300f, 1.2f, 20f, 0f);
            private readonly Stopwatch _stopwatch = new();
            private readonly System.Windows.Forms.Timer _timer;
            private readonly WebView2 _webView;
            private readonly Label _fallbackLabel;
            private bool _islandVisible;
            private bool _webViewFailed;
            private long _lastTick;
            private long _lastFallbackUpdate;

            public IslandForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TopMost = true;
                StartPosition = FormStartPosition.Manual;
                Size = new Size(1, 1);
                BackColor = Color.Black;

                _webView = new WebView2 { Dock = DockStyle.Fill };
                Controls.Add(_webView);

                _fallbackLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9f),
                    Visible = false
                };
                Controls.Add(_fallbackLabel);

                _timer = new System.Windows.Forms.Timer { Interval = 15 };
                _timer.Tick += OnTick;
            }

            public void ShowIsland() => _islandVisible = true;
            public void HideIsland() => _islandVisible = false;

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                long style = GetWindowLongPtr(Handle, GwlExstyle).ToInt64();
                style |= WsExLayered | WsExTransparent | WsExToolwindow | WsExNoactivate;
                SetWindowLongPtr(Handle, GwlExstyle, new IntPtr(style));
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                _stopwatch.Start();
                _timer.Start();
                InitWebView2Async();
            }

            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                _timer.Stop();
                base.OnFormClosed(e);
            }

            private async void InitWebView2Async()
            {
                try
                {
                    TryLoadWebView2Loader();
                    string userDataFolder = Path.Combine(MainFolder.GetMainFolder(), "WebView2");
                    var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                    _webView.DefaultBackgroundColor = Color.Black;
                    await _webView.EnsureCoreWebView2Async(env);
                    _webView.NavigateToString(BuildHtml());
                }
                catch (Exception ex)
                {
                    _webViewFailed = true;
                    _webView.Visible = false;
                    _fallbackLabel.Visible = true;
                    Console.WriteLine("[Gui] WebView2 unavailable, using fallback label: " + ex.Message);
                }
            }

            private void OnTick(object sender, EventArgs e)
            {
                long now = _stopwatch.ElapsedMilliseconds;
                float dt = (now - _lastTick) / 1000f;
                _lastTick = now;
                if (dt <= 0f) return;
                dt = Math.Min(dt, 0.0333333f);

                _widthAnim.TargetValue = _islandVisible ? TargetWidth : 0f;
                _heightAnim.TargetValue = _islandVisible ? TargetHeight : 0f;
                _widthAnim.Update(dt);
                _heightAnim.Update(dt);

                IntPtr gameWnd = Process.GetCurrentProcess().MainWindowHandle;
                bool gameVisible = gameWnd != IntPtr.Zero && !IsIconic(gameWnd);
                bool shouldShow = _islandVisible && gameVisible
                    || !_islandVisible && (_widthAnim.CurrentValue >= 2f || _heightAnim.CurrentValue >= 2f);

                if (!shouldShow)
                {
                    SetWindowPos(Handle, HwndTopmost, 0, 0, 0, 0,
                        SwpNomove | SwpNosize | SwpNoactivate | SwpHidewindow);
                    return;
                }

                int w = Math.Max(1, (int)Math.Round(_widthAnim.CurrentValue));
                int h = Math.Max(1, (int)Math.Round(_heightAnim.CurrentValue));

                int x = 0, y = 0;
                if (gameVisible && GetWindowRect(gameWnd, out RECT r))
                {
                    x = r.Left + (r.Right - r.Left - w) / 2;
                    y = r.Top + TopMargin;
                }

                SetWindowPos(Handle, HwndTopmost, x, y, w, h,
                    SwpNoactivate | SwpShowwindow);

                int diameter = Math.Min(w, h);
                SetWindowRgn(Handle, CreateRoundRectRgn(0, 0, w + 1, h + 1, diameter, diameter), true);

                if (_webViewFailed && now - _lastFallbackUpdate >= 500)
                {
                    _lastFallbackUpdate = now;
                    _fallbackLabel.Text = FallbackText();
                }
            }

            private static string ClientVersion()
            {
                var v = typeof(InjectEntry).Assembly.GetName().Version;
                return $"v{v.Major}.{v.Minor}.{v.Build}";
            }

            private static string FallbackText()
                => "AliceInCradleHack " + ClientVersion() + " | " + DateTime.Now.ToString("HH:mm:ss");

            private static string BuildHtml()
                => IslandHtml.Replace("{{VERSION}}", ClientVersion());

            private const string IslandHtml =
                "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>" +
                "html,body{margin:0;padding:0;width:100%;height:100%;overflow:hidden;background:#000;" +
                "font-family:'Segoe UI',sans-serif;color:#fff;user-select:none;cursor:default;}" +
                "#island{display:flex;align-items:center;justify-content:center;gap:8px;" +
                "width:100%;height:100%;font-size:12px;white-space:nowrap;}" +
                ".brand{color:#7ee787;font-weight:600;}.sep{opacity:.35;}.dim{opacity:.75;}" +
                "</style></head><body><div id=\"island\">" +
                "<span class=\"brand\">AliceInCradleHack</span>" +
                "<span class=\"sep\">|</span>" +
                "<span class=\"dim\">{{VERSION}}</span>" +
                "<span class=\"sep\">|</span>" +
                "<span id=\"time\" class=\"dim\">--:--:--</span>" +
                "</div><script>" +
                "function pad(n){return (n<10?'0':'')+n;}" +
                "function tick(){var d=new Date();" +
                "document.getElementById('time').textContent=" +
                "pad(d.getHours())+':'+pad(d.getMinutes())+':'+pad(d.getSeconds());}" +
                "tick();setInterval(tick,1000);" +
                "</script></body></html>";

            private static void TryLoadWebView2Loader()
            {
                try
                {
                    string mainFolder = MainFolder.GetMainFolder();
                    string assemblyDir = Path.GetDirectoryName(typeof(InjectEntry).Assembly.Location) ?? "";
                    string[] candidates =
                    {
                        Path.Combine(mainFolder, "lib", "WebView2Loader.dll"),
                        Path.Combine(mainFolder, "WebView2Loader.dll"),
                        Path.Combine(assemblyDir, "runtimes", "win-x64", "native", "WebView2Loader.dll"),
                        Path.Combine(assemblyDir, "WebView2Loader.dll")
                    };
                    foreach (string loader in candidates)
                    {
                        if (!File.Exists(loader)) continue;
                        SetDllDirectory(Path.GetDirectoryName(loader));
                        LoadLibrary(loader);
                        return;
                    }
                }
                catch { }
            }

            private const int GwlExstyle = -20;
            private const long WsExLayered = 0x00080000;
            private const long WsExTransparent = 0x00000020;
            private const long WsExToolwindow = 0x00000080;
            private const long WsExNoactivate = 0x08000000;
            private const uint SwpNomove = 0x0002;
            private const uint SwpNosize = 0x0001;
            private const uint SwpNoactivate = 0x0010;
            private const uint SwpShowwindow = 0x0040;
            private const uint SwpHidewindow = 0x0080;
            private static readonly IntPtr HwndTopmost = new(-1);

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT { public int Left, Top, Right, Bottom; }

            [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
            private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
            private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int x, int y, int cx, int cy, uint flags);

            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            private static extern bool IsIconic(IntPtr hWnd);

            [DllImport("gdi32.dll")]
            private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int w, int h);

            [DllImport("user32.dll")]
            private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetDllDirectory(string lpPathName);
        }
    }
}
