using AliceInCradleHack.command;
using AliceInCradleHack.extension;
using AliceInCradleHack.module;
using AliceInCradleHack.patch;
using AliceInCradleHack.utils.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AliceInCradleHack
{
    public static partial class Client
    {
        public const string ClientName = "AliceInCradleHack";
        public const string VersionType = "beta";
        public const string Version = "0.0.1";

        private static readonly object _lifecycleLock = new();
        private static readonly List<IClientComponent> _components = new();
        private static bool _initialized;
        private static bool _disposed;

        private static void PrintSplash()
        {
            Console.WriteLine(
                "           _ _          _____        _____               _ _      _    _            _    \r\n" +
                "     /\\   | (_)        |_   _|      / ____|             | | |    | |  | |          | |   \r\n" +
                "    /  \\  | |_  ___ ___  | |  _ __ | |     _ __ __ _  __| | | ___| |__| | __ _  ___| | __\r\n" +
                "   / /\\ \\  | | |/ __/ _ \\ | | | '_ \\| |    | '__/ _` |/ _` | |/ _ \\  __  |/ _` |/ __| |/ /\r\n" +
                "  / ____ \\| | | (_|  __/_| |_| | | | |____| | | (_| | (_| | |  __/ |  | | (_| | (__|   < \r\n" +
                " /_/    \\_\\_|_|\\___\\___|_____|_| |_|\\_____|_|  \\__,_|\\__,_|_|\\___|_|  |_|\\__,_|\\___|_|\\_\\\r\n" +
                "                                                                                         \r\n" +
                "                                                                                         "
            );
        }

        public static void Initialize()
        {
            lock (_lifecycleLock)
            {
                if (_initialized) return;
                if (_disposed)
                {
                    Console.WriteLine("AliceInCradleHack has already been shut down.");
                    return;
                }
                _initialized = true;
            }

            try
            {
                ConsoleHost.Instance.Initialize();
                PrintSplash();

                Log.Info("Initializing...");

                Log.Info("Resolving main folder...");
                string mainFolder = MainFolder.GetMainFolder();
                Log.Info("Main folder: " + mainFolder);

                Log.Info("Registering dependency resolver...");
                DependencyResolver.Instance.RegisterDirectory(Path.Combine(mainFolder, "lib"));

                Log.Initialize();
                Log.Info("Runtime: " + RuntimeInformation.FrameworkDescription + " (CLR " + Environment.Version + ")");

                Log.Info("Applying patches...");
                Start(PatchManager.Instance);

                Log.Info("Registering commands...");
                Start(CommandManager.Instance);

                Log.Info("Initializing modules...");
                Start(ModuleManager.Instance);

                Log.Info("Loading extensions...");
                Start(ExtensionManager.Instance);

                Log.Info("Initialization complete.");
                Log.Info("Injection successful!");
            }
            catch (Exception ex)
            {
                Log.Error("Injection failed", ex);
                Log.Error("Please eject the DLL.");
                DisposeComponents();
                lock (_lifecycleLock) _initialized = false;
            }
        }

        /// <summary>
        /// Shuts the hack down in reverse order of initialization: extensions, modules,
        /// commands, patches, then the log file and the console. Note: ejecting the DLL with
        /// SharpInjector crashes the host process, reason unknown.
        /// </summary>
        public static void Dispose()
        {
            lock (_lifecycleLock)
            {
                if (_disposed) return;
                _disposed = true;
            }

            try
            {
                Log.Info("Shutting down...");
                DisposeComponents();
            }
            finally
            {
                Log.Dispose();
                ConsoleHost.Instance.Dispose();
            }
        }

        private static void Start(IClientComponent component)
        {
            component.Initialize();
            lock (_lifecycleLock)
            {
                _components.Add(component);
            }
        }

        private static void DisposeComponents()
        {
            IClientComponent[] components;
            lock (_lifecycleLock)
            {
                components = _components.ToArray();
                _components.Clear();
            }

            for (int i = components.Length - 1; i >= 0; i--)
            {
                try
                {
                    components[i].Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to dispose {components[i].GetType().Name}", ex);
                }
            }
        }
    }
}
