using AliceInCradleHack.utils.client;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// The "Log" root config (configs/log.json). Values are read live by
    /// <see cref="Log"/> on every write, so edits via commands or the WebUI apply immediately.
    /// </summary>
    public static class LogConfig
    {
        public static EnumChoiceValue<LogLevel> Level { get; private set; }
        public static Value<bool> ConsoleOutput { get; private set; }
        public static Value<bool> FileOutput { get; private set; }
        public static Value<bool> Colors { get; private set; }
        public static EnumChoiceValue<LogFileMode> FileMode { get; private set; }

        /// <summary>
        /// Creates, registers and loads the config. Called once by <see cref="Log.Init"/>.
        /// </summary>
        public static Config Register()
        {
            var config = new Config("Log", "Logger settings");
            Level = config.EnumChoice("Level", LogLevel.Info, "Minimum log level; messages below it are discarded.");
            ConsoleOutput = config.Boolean("ConsoleOutput", true, "Write logs to the console.");
            FileOutput = config.Boolean("FileOutput", true, "Write logs to a file under AliceInCradleHack/logs.");
            Colors = config.Boolean("Colors", true, "Colorize console output by log level.");
            FileMode = config.EnumChoice("FileMode", LogFileMode.Daily, "Log file naming: Single (hack.log), Daily (hack-yyyyMMdd.log), PerRun (hack-yyyyMMdd-HHmmss.log).");

            ConfigSystem.Root(config);
            ConfigSystem.Load(config);
            return config;
        }
    }
}
