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
        /// Serializes every registered root config into a single JSON object keyed by config name.
        /// </summary>
        public static string ExportAllToJson()
        {
            var all = new JObject();
            foreach (var config in _configs)
                all[config.Name] = config.ToJToken();
            return all.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Applies a single merged JSON object (as produced by <see cref="ExportAllToJson"/>) to the
        /// registered root configs and stores them. Unknown entries are skipped with a warning.
        /// </summary>
        public static bool ImportAllFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                var all = JObject.Parse(json);
                bool success = true;
                foreach (var property in all.Properties())
                {
                    var config = FindConfig(property.Name);
                    if (config == null)
                    {
                        Log.Warn($"Config '{property.Name}' not found, skipping...");
                        success = false;
                        continue;
                    }
                    if (property.Value is not JObject configObj) continue;
                    config.FromJToken(configObj);
                    Store(config);
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error("Unable to import config JSON", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves all root configs into a single file named "{name}.json" inside the configs folder.
        /// Returns the file name (without extension), or null on failure, an invalid name, or a name
        /// that collides with a registered config.
        /// </summary>
        public static string SaveAllToFile(string name)
        {
            string safeName = SanitizeFileName(name);
            if (safeName == null) return null;
            if (FindConfig(safeName) != null)
            {
                Log.Warn($"Cannot save config as '{safeName}': name conflicts with a registered config");
                return null;
            }
            try
            {
                string path = Path.Combine(ConfigsFolder, safeName + ".json");
                File.WriteAllText(path, ExportAllToJson(), Encoding.UTF8);
                Log.Info($"Saved config to {path}");
                return safeName;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to save config file '{name}'", ex);
                return null;
            }
        }

        /// <summary>
        /// Loads a single-file config "{name}.json" from the configs folder and applies it.
        /// Names that collide with registered configs are rejected.
        /// </summary>
        public static bool LoadAllFromFile(string name)
        {
            string safeName = SanitizeFileName(name);
            if (safeName == null) return false;
            if (FindConfig(safeName) != null)
            {
                Log.Warn($"Cannot load config '{safeName}': name conflicts with a registered config");
                return false;
            }
            try
            {
                string path = Path.Combine(ConfigsFolder, safeName + ".json");
                if (!File.Exists(path))
                {
                    Log.Warn($"Config file does not exist: {path}");
                    return false;
                }
                return ImportAllFromJson(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to load config file '{name}'", ex);
                return false;
            }
        }

        /// <summary>
        /// Lists single-file configs (excluding the per-config runtime files) saved in the configs folder.
        /// </summary>
        public static string[] ListSavedFiles()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var config in _configs)
                names.Add(config.Name.ToLowerInvariant());
            var result = new List<string>();
            try
            {
                if (!Directory.Exists(ConfigsFolder)) return Array.Empty<string>();
                foreach (var file in Directory.GetFiles(ConfigsFolder, "*.json"))
                {
                    string baseName = Path.GetFileNameWithoutExtension(file);
                    if (!names.Contains(baseName.ToLowerInvariant()))
                        result.Add(baseName);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to list saved config files", ex);
            }
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result.ToArray();
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var invalid = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(name.Trim());
            foreach (char c in invalid)
                builder.Replace(c, '_');
            string safe = builder.ToString();
            if (safe.Length == 0) return null;
            return safe;
        }

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
