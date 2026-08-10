using AliceInCradleHack.command;
using AliceInCradleHack.extension;
using AliceInCradleHack.module;
using AliceInCradleHack.patch;
using AliceInCradleHack.utils.client;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace AliceInCradleHack
{
    public class InjectEntry
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private static readonly Thread _injectThread = new(InjectTask);

        // Entry point invoked by the injector: AliceInCradleHack.InjectEntry:Inject()
        private static void Inject()
        {
            _injectThread.SetApartmentState(ApartmentState.STA);
            _injectThread.Start();
        }

        private static void SetupConsole()
        {
            AllocConsole();

            // Redirect input and output to the new console.
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.Title = "AliceInCradleHack Console";

            // Swallow Ctrl+C so the host game process is not terminated.
            Console.CancelKeyPress += (sender, e) => e.Cancel = true;
        }

        private static void PrintSplash()
        {
            Console.WriteLine(
                "           _ _          _____        _____               _ _      _    _            _    \r\n" +
                "     /\\   | (_)        |_   _|      / ____|             | | |    | |  | |          | |   \r\n" +
                "    /  \\  | |_  ___ ___  | |  _ __ | |     _ __ __ _  __| | | ___| |__| | __ _  ___| | __\r\n" +
                "   / /\\ \\ | | |/ __/ _ \\ | | | '_ \\| |    | '__/ _` |/ _` | |/ _ \\  __  |/ _` |/ __| |/ /\r\n" +
                "  / ____ \\| | | (_|  __/_| |_| | | | |____| | | (_| | (_| | |  __/ |  | | (_| | (__|   < \r\n" +
                " /_/    \\_\\_|_|\\___\\___|_____|_| |_|\\_____|_|  \\__,_|\\__,_|_|\\___|_|  |_|\\__,_|\\___|_|\\_\\\r\n" +
                "                                                                                         \r\n" +
                "                                                                                         "
            );
        }

        // Loads dependencies from mainFolder\lib.
        private static void RegisterAssemblyResolver(string mainFolder)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                string assemblyPath = Path.Combine(mainFolder + "\\lib", assemblyName);

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                return null;
            };
        }

        private static void InjectTask()
        {
            try
            {
                SetupConsole();
                PrintSplash();

                Log.Info("Initializing...");

                Log.Info("Resolving main folder...");
                string mainFolder = MainFolder.GetMainFolder();
                Log.Info("Main folder: " + mainFolder);

                Log.Info("Registering dependency resolver...");
                RegisterAssemblyResolver(mainFolder);

                Log.Init();

                Log.Info("Runtime: " + RuntimeInformation.FrameworkDescription + " (CLR " + Environment.Version + ")");

                Log.Info("Applying patches...");
                PatchManager.Instance.Initialize();

                Log.Info("Registering commands...");
                CommandManager.Instance.Initialize();

                Log.Info("Initializing modules...");
                ModuleManager.Instance.Initialize();

                Log.Info("Loading extensions...");
                string extensionsDir = Path.Combine(mainFolder, "Extensions");
                ExtensionManager.Instance.LoadFromDirectory(extensionsDir);

                Log.Info("Initialization complete.");
                Log.Info("Injection successful!");

                CommandManager.Instance.RunCommandLoop();
            }
            catch (Exception ex)
            {
                Log.Error("Injection failed", ex);
                Log.Error("Please eject the DLL.");
            }
        }

        // Note: ejecting the DLL with SharpInjector crashes the host process, reason unknown.
        private static void Eject()
        {
            Log.Shutdown();
            FreeConsole();
        }
    }
}
