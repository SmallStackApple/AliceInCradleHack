using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AliceInCradleHack.extension
{
    /// <summary>
    /// Extension manager (singleton). Loads extension DLLs from a directory and manages their lifecycle.
    /// </summary>
    public class ExtensionManager
    {
        private readonly Dictionary<string, Extension> _extensions = new();

        private static readonly Lazy<ExtensionManager> _lazyInstance = new(() => new ExtensionManager());
        public static ExtensionManager Instance => _lazyInstance.Value;

        private ExtensionManager() { }

        public void LoadFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Extension directory not found: {directory}");
                return;
            }

            Console.WriteLine($"Scanning extensions from: {directory}");

            foreach (var dllPath in Directory.GetFiles(directory, "*.dll"))
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
                    Console.WriteLine($"Failed to load {fileName}: {ex.Message}");
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
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var name = new AssemblyName(args.Name).Name + ".dll";
                    var path = Path.Combine(libDir, name);
                    return File.Exists(path) ? Assembly.LoadFrom(path) : null;
                };
            }

            var assembly = Assembly.LoadFrom(dllPath);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(Extension)))
                    continue;

                try
                {
                    var ext = (Extension)Activator.CreateInstance(type);
                    RegisterExtension(ext);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to instantiate extension {type.FullName}: {ex.Message}");
                }
            }
        }

        public void RegisterExtension(Extension ext)
        {
            if (ext == null)
                throw new ArgumentNullException(nameof(ext));

            if (_extensions.ContainsKey(ext.Name))
            {
                Console.WriteLine($"Extension '{ext.Name}' already registered, skipping.");
                return;
            }

            try
            {
                ext.Load();
                ext.IsLoaded = true;
                _extensions[ext.Name] = ext;
                Console.WriteLine($"Loaded extension: {ext.Name} v{ext.GetType().Assembly.GetName().Version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load extension '{ext.Name}': {ex.Message}");
            }
        }

        public void UnloadExtension(string name)
        {
            if (!_extensions.TryGetValue(name, out var ext)) return;

            try
            {
                ext.Unload();
                ext.IsLoaded = false;
                _extensions.Remove(name);
                Console.WriteLine($"Unloaded extension: {name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to unload extension '{name}': {ex.Message}");
            }
        }

        public T GetExtension<T>(string name) where T : Extension
        {
            _extensions.TryGetValue(name, out var ext);
            return ext as T;
        }

        public IEnumerable<Extension> GetAllExtensions() => _extensions.Values;

        public void UnloadAll()
        {
            foreach (var name in _extensions.Keys.ToArray())
                UnloadExtension(name);
        }
    }
}
