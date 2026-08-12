using AliceInCradleHack.utils.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AliceInCradleHack.extension
{
    /// <summary>
    /// Extension manager (singleton). Loads extension DLLs from a directory and manages their
    /// lifecycle. <see cref="Initialize"/> scans &lt;mainFolder&gt;\Extensions;
    /// <see cref="Dispose"/> unloads every loaded extension in reverse registration order.
    /// Extensions run in the default AppDomain, so they can call the game and the client's
    /// managers directly. Note: because assemblies are loaded with <see cref="Assembly.LoadFrom"/>
    /// and never into a dedicated AppDomain, the DLL files stay resident until the host process
    /// exits — "unloading" only releases the extension's managed resources.
    /// </summary>
    public class ExtensionManager : IClientComponent
    {
        private readonly Dictionary<string, Extension> _extensions = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _loadOrder = new();
        private readonly Dictionary<string, string> _extensionLibDirs = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _registeredLibDirs = new(StringComparer.OrdinalIgnoreCase);
        private bool _initialized;

        private static readonly Lazy<ExtensionManager> _lazyInstance = new(() => new ExtensionManager());
        public static ExtensionManager Instance => _lazyInstance.Value;

        private ExtensionManager() { }

        /// <summary>
        /// Scans &lt;mainFolder&gt;\Extensions for extension DLLs. Idempotent.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            string extensionsDir = Path.Combine(MainFolder.GetMainFolder(), "Extensions");
            LoadFromDirectory(extensionsDir);
            _initialized = true;
        }

        /// <summary>
        /// Scans a directory for extension DLLs and registers every Extension type found.
        /// </summary>
        public void LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Log.Warn($"Extension directory not found: {directory}");
                return;
            }

            Log.Info($"Scanning extensions from: {directory}");

            foreach (var dllPath in Directory.GetFiles(directory, "*.dll").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var fileName = Path.GetFileName(dllPath);
                if (fileName.Equals("AliceInCradleHack.dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    LoadAssembly(dllPath);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to load {fileName}", ex);
                }
            }
        }

        private void LoadAssembly(string dllPath)
        {
            var dir = Path.GetDirectoryName(dllPath);
            var libDir = Path.Combine(dir, "lib");

            // Resolve extension dependencies from a "lib" folder next to the extension DLL.
            if (Directory.Exists(libDir))
            {
                lock (_registeredLibDirs)
                {
                    if (_registeredLibDirs.Add(libDir))
                        DependencyResolver.Instance.RegisterDirectory(libDir);
                }
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(dllPath);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load assembly from {dllPath}", ex);
                return;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException rtle)
            {
                // Types whose dependencies failed to resolve are null; keep what is loadable.
                types = rtle.Types.Where(t => t != null).ToArray();
                foreach (var loaderException in rtle.LoaderExceptions ?? Array.Empty<Exception>())
                    Log.Error($"Type resolution failed in {Path.GetFileName(dllPath)}", loaderException);
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsNotPublic || type.ContainsGenericParameters || !type.IsSubclassOf(typeof(Extension)))
                    continue;

                try
                {
                    if (Activator.CreateInstance(type) is Extension ext)
                        RegisterExtension(ext, libDir);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to instantiate extension {type.FullName}", ex);
                }
            }
        }

        public void RegisterExtension(Extension ext)
        {
            RegisterExtension(ext, null);
        }

        private void RegisterExtension(Extension ext, string libDir)
        {
            if (ext == null)
                throw new ArgumentNullException(nameof(ext));

            if (string.IsNullOrWhiteSpace(ext.Name))
            {
                Log.Error($"Extension of type {ext.GetType().FullName} has an empty name, skipping.");
                return;
            }

            if (_extensions.ContainsKey(ext.Name))
            {
                Log.Warn($"Extension '{ext.Name}' already registered, skipping.");
                return;
            }

            try
            {
                ext.Initialize();
                ext.IsLoaded = true;
                _extensions[ext.Name] = ext;
                _loadOrder.Add(ext.Name);
                if (!string.IsNullOrEmpty(libDir))
                    _extensionLibDirs[ext.Name] = libDir;
                Log.Info($"Loaded extension: {ext.Name} v{ext.GetType().Assembly.GetName().Version}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load extension '{ext.Name}'", ex);
            }
        }

        public void UnloadExtension(string name)
        {
            if (!_extensions.TryGetValue(name, out var ext)) return;

            try
            {
                ext.Dispose();
                ext.IsLoaded = false;
                Log.Info($"Unloaded extension: {name}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to unload extension '{name}'", ex);
            }
            finally
            {
                _extensions.Remove(name);
                _loadOrder.Remove(name);
                if (_extensionLibDirs.TryGetValue(name, out var libDir))
                {
                    _extensionLibDirs.Remove(name);
                    lock (_registeredLibDirs)
                    {
                        _registeredLibDirs.Remove(libDir);
                    }
                    DependencyResolver.Instance.UnregisterDirectory(libDir);
                }
            }
        }

        public T GetExtension<T>(string name) where T : Extension
        {
            _extensions.TryGetValue(name, out var ext);
            return ext as T;
        }

        public IEnumerable<Extension> GetAllExtensions() => _extensions.Values.ToArray();

        /// <summary>
        /// Unloads every extension in reverse registration order. Safe to call more than once.
        /// </summary>
        public void Dispose()
        {
            if (_extensions.Count == 0 && !_initialized) return;

            for (int i = _loadOrder.Count - 1; i >= 0; i--)
            {
                UnloadExtension(_loadOrder[i]);
            }
            _initialized = false;
        }
    }
}
