using AliceInCradleHack.config;
using System;
using System.IO;
using System.Text;

namespace AliceInCradleHack.utils.client
{
    public enum LogLevel { Debug, Info, Warn, Error }

    public enum LogFileMode { Single, Daily, PerRun }

    /// <summary>
    /// Unified logger. Writes timestamped, leveled lines to the console and/or a log file.
    /// Behavior is controlled by the "Log" config (see <see cref="LogConfig"/>) and changes apply live.
    /// </summary>
    public static class Log
    {
        private static readonly object _lock = new();
        private static StreamWriter _fileWriter;
        private static string _logFolder;
        private static bool _initialized;

        /// <summary>
        /// Registers and loads the "Log" config, then opens the file sink if enabled.
        /// Safe to call multiple times; only the first call has an effect.
        /// </summary>
        public static void Init()
        {
            lock (_lock)
            {
                if (_initialized) return;
                _initialized = true;
            }

            LogConfig.Register();
            _logFolder = Path.Combine(ConfigSystem.RootFolder, "logs");

            LogConfig.FileOutput.OnChanged(_ => ReopenWriter());
            LogConfig.FileMode.OnChanged(_ => ReopenWriter());
            ReopenWriter();
        }

        public static void Debug(string message) => Write(LogLevel.Debug, message);

        public static void Info(string message) => Write(LogLevel.Info, message);

        public static void Warn(string message) => Write(LogLevel.Warn, message);

        public static void Error(string message) => Write(LogLevel.Error, message);

        public static void Error(string message, Exception ex) =>
            Write(LogLevel.Error, $"{message}: {ex}");

        public static void Shutdown()
        {
            lock (_lock)
            {
                CloseWriterNoLock();
            }
        }

        private static void Write(LogLevel level, string message)
        {
            bool console = true, colors = true;
            StreamWriter file = null;

            if (_initialized)
            {
                if (level < LogConfig.Level.Get()) return;
                console = LogConfig.ConsoleOutput.Get();
                colors = LogConfig.Colors.Get();
                if (LogConfig.FileOutput.Get()) file = _fileWriter;
            }

            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level.ToString().ToUpperInvariant()}] {message}";

            lock (_lock)
            {
                if (console)
                {
                    if (colors)
                    {
                        ConsoleColor previous = Console.ForegroundColor;
                        Console.ForegroundColor = ColorFor(level);
                        Console.WriteLine(line);
                        Console.ForegroundColor = previous;
                    }
                    else
                    {
                        Console.WriteLine(line);
                    }
                }

                if (file != null)
                {
                    try { file.WriteLine(line); }
                    catch { /* never let logging crash the host process */ }
                }
            }
        }

        private static ConsoleColor ColorFor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return ConsoleColor.DarkGray;
                case LogLevel.Warn: return ConsoleColor.Yellow;
                case LogLevel.Error: return ConsoleColor.Red;
                default: return ConsoleColor.Gray;
            }
        }

        private static void ReopenWriter()
        {
            lock (_lock)
            {
                CloseWriterNoLock();
                if (!LogConfig.FileOutput.Get()) return;

                try
                {
                    Directory.CreateDirectory(_logFolder);
                    string fileName;
                    switch (LogConfig.FileMode.Get())
                    {
                        case LogFileMode.Single:
                            fileName = "hack.log";
                            break;
                        case LogFileMode.PerRun:
                            fileName = $"hack-{DateTime.Now:yyyyMMdd-HHmmss}.log";
                            break;
                        default:
                            fileName = $"hack-{DateTime.Now:yyyyMMdd}.log";
                            break;
                    }
                    _fileWriter = new StreamWriter(Path.Combine(_logFolder, fileName), true, Encoding.UTF8) { AutoFlush = true };
                }
                catch (Exception ex)
                {
                    _fileWriter = null;
                    Warn($"Failed to open log file: {ex.Message}");
                }
            }
        }

        private static void CloseWriterNoLock()
        {
            try { _fileWriter?.Dispose(); } catch { }
            _fileWriter = null;
        }
    }
}
