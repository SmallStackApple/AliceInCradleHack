using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AliceInCradleHack.utils.client
{
    /// <summary>
    /// Owns the injected console: allocates it on <see cref="Initialize"/> and frees it on
    /// <see cref="Dispose"/>. Redirects standard input/output so the command loop and the
    /// logger have a console to work with.
    /// </summary>
    public class ConsoleHost : IClientComponent
    {
        private static readonly Lazy<ConsoleHost> _lazyInstance = new(() => new ConsoleHost());
        public static ConsoleHost Instance => _lazyInstance.Value;

        private bool _initialized;

        private ConsoleHost() { }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public void Initialize()
        {
            if (_initialized) return;
            AllocConsole();

            // Redirect input and output to the new console.
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.Title = "AliceInCradleHack Console";

            // Swallow Ctrl+C so the host game process is not terminated.
            Console.CancelKeyPress += (sender, e) => e.Cancel = true;

            _initialized = true;
        }

        public void Dispose()
        {
            if (!_initialized) return;
            _initialized = false;
            FreeConsole();
        }
    }
}
