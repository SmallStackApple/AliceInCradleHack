using AliceInCradleHack.utils.client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// Registry of root configs. Handles loading, storing (atomically) and zip backups.
    /// Config files live in &lt;mainFolder&gt;\AliceInCradleHack\configs.
    /// </summary>
    public static class ConfigSystem
    {
        private static readonly List<Config> _configs = new List<Config>();
        private static string _rootFolder;

        public static IReadOnlyList<Config> Configs => _configs;

        public static string RootFolder
        {
            get
            {
                if (_rootFolder == null)
                {
                    _rootFolder = Path.Combine(utils.client.MainFolder.GetMainFolder(), "AliceInCradleHack");
                    Directory.CreateDirectory(_rootFolder);
                    Directory.CreateDirectory(ConfigsFolder);
                    Directory.CreateDirectory(BackupFolder);
                }
                return _rootFolder;
            }
        }

        public static string ConfigsFolder => Path.Combine(RootFolder, "configs");

        public static string BackupFolder => Path.Combine(RootFolder, "backups");

        /// <summary>
        /// Registers a root config. Does not load it; call <see cref="Load"/> or <see cref="LoadAll"/>.
        /// </summary>
        public static T Root<T>(T config) where T : Config
        {
            if (!_configs.Contains(config))
                _configs.Add(config);
            return config;
        }

        internal static void EnsureRegistered(Config config)
        {
            if (!_configs.Contains(config))
                throw new InvalidOperationException($"Config '{config.Name}' is not registered in ConfigSystem");
        }

        public static Config FindConfig(string name)
        {
            foreach (var config in _configs)
                if (config.MatchesName(name)) return config;
            return null;
        }

        /// <summary>
        /// Finds a value by its full path: "ConfigName.Path.To.Value" (case-insensitive).
        /// </summary>
        public static Value FindValueByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            var trimmed = key.Trim();
            int dot = trimmed.IndexOf('.');
            string configName = dot < 0 ? trimmed : trimmed.Substring(0, dot);
            string rest = dot < 0 ? "" : trimmed.Substring(dot + 1);

            var config = FindConfig(configName);
            return config?.GetNodeByPath(rest);
        }

        // Loading / storing

        public static void LoadAll()
        {
            foreach (var config in _configs)
                Load(config);
        }

        public static void Load(Config config)
        {
            string file = config.JsonFile;
            try
            {
                if (File.Exists(file))
                {
                    var obj = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                    string storedName = obj["name"]?.ToString();
                    if (storedName != null && !config.MatchesName(storedName))
                        Log.Warn($"Config name mismatch: expected '{config.Name}', file contains '{storedName}'");
                    config.FromJToken(obj);
                    Log.Info($"Loaded config '{config.Name}'.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to load config '{config.Name}'", ex);
            }

            // Store back so new values are written and stale entries are cleaned up.
            Store(config);
        }

        public static void StoreAll()
        {
            foreach (var config in _configs)
                Store(config);
        }

        /// <summary>
        /// Writes the config to a temp file, then renames it over the target file,
        /// so a crash mid-write cannot corrupt the existing config.
        /// </summary>
        public static void Store(Config config)
        {
            try
            {
                string tmp = config.JsonTmpFile;
                File.WriteAllText(tmp, config.ToJToken().ToString(Formatting.Indented), Encoding.UTF8);

                string file = config.JsonFile;
                if (File.Exists(file))
                    File.Delete(file);
                File.Move(tmp, file);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to store config '{config.Name}'", ex);
            }
        }

        // Backup / restore

        /// <summary>
        /// Creates a zip backup of all config files. Returns the backup file name (without extension).
        /// </summary>
        public static string Backup(string fileName)
        {
            Directory.CreateDirectory(BackupFolder);
            string zipPath = Path.Combine(BackupFolder, fileName + ".zip");
            int suffix = 1;
            string baseName = fileName;
            while (File.Exists(zipPath))
                zipPath = Path.Combine(BackupFolder, $"{baseName}_{suffix++}.zip");

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var config in _configs)
                {
                    string file = config.JsonFile;
                    if (File.Exists(file))
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
            Log.Info($"Backup created: {zipPath}");
            return Path.GetFileNameWithoutExtension(zipPath);
        }

        /// <summary>
        /// Restores config files from a zip backup and reloads them.
        /// </summary>
        public static bool Restore(string fileName)
        {
            string zipPath = Path.Combine(BackupFolder, fileName + ".zip");
            if (!File.Exists(zipPath))
            {
                Log.Warn($"Backup file does not exist: {zipPath}");
                return false;
            }

            StoreAll();
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string target = Path.Combine(ConfigsFolder, entry.Name);
                    entry.ExtractToFile(target, overwrite: true);
                }
            }
            LoadAll();
            Log.Info($"Backup restored: {zipPath}");
            return true;
        }
    }
}
