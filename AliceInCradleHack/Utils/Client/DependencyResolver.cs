using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AliceInCradleHack.utils.client
{
    /// <summary>
    /// Centralized AppDomain.AssemblyResolve handler. Probe directories are registered by
    /// <see cref="Client"/> (mainFolder\lib) and by the extension loader (each extension's
    /// lib folder). Registered once instead of accumulating a handler per loaded DLL.
    /// </summary>
    public class DependencyResolver
    {
        private static readonly Lazy<DependencyResolver> _lazyInstance = new(() => new DependencyResolver());
        public static DependencyResolver Instance => _lazyInstance.Value;

        private readonly object _lock = new();
        private readonly List<string> _probeDirectories = new();

        private DependencyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <summary>
        /// Adds a directory to the probe list. Idempotent.
        /// </summary>
        public void RegisterDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;
            lock (_lock)
            {
                if (!_probeDirectories.Contains(directory))
                    _probeDirectories.Add(directory);
            }
        }

        /// <summary>
        /// Removes a directory from the probe list. Safe to call more than once.
        /// </summary>
        public void UnregisterDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;
            lock (_lock)
            {
                _probeDirectories.Remove(directory);
            }
        }

        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string fileName = new AssemblyName(args.Name).Name + ".dll";

            string[] directories;
            lock (_lock)
            {
                directories = _probeDirectories.ToArray();
            }

            foreach (var directory in directories)
            {
                string path = Path.Combine(directory, fileName);
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }
            return null;
        }
    }
}
