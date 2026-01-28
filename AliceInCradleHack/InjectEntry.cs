using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AliceInCradleHack
{
    public class InjectEntry
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        
        static void Inject()
        {
            CommandManager commandManager = CommandManager.Instance;
            ModuleManager moduleManager = ModuleManager.Instance;
            EventManager eventManager = EventManager.Instance;
            try
            {
                AllocConsole();
                //redirect input and output
                Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput()));
                Console.Title = "AliceInCradleHack Console";

                //cancel ctrl+c handling to avoid terminating the host process
                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true;
                };
                
                Console.ForegroundColor = ConsoleColor.Green;
                //splash :P
                Console.WriteLine(
                    "           _ _          _____        _____               _ _      _    _            _    \r\n"+
                    "     /\\   | (_)        |_   _|      / ____|             | | |    | |  | |          | |   \r\n"+
                    "    /  \\  | |_  ___ ___  | |  _ __ | |     _ __ __ _  __| | | ___| |__| | __ _  ___| | __\r\n"+
                    "   / /\\ \\ | | |/ __/ _ \\ | | | '_ \\| |    | '__/ _` |/ _` | |/ _ \\  __  |/ _` |/ __| |/ /\r\n"+
                    "  / ____ \\| | | (_|  __/_| |_| | | | |____| | | (_| | (_| | |  __/ |  | | (_| | (__|   < \r\n"+
                    " /_/    \\_\\_|_|\\___\\___|_____|_| |_|\\_____|_|  \\__,_|\\__,_|_|\\___|_|  |_|\\__,_|\\___|_|\\_\\\r\n"+
                    "                                                                                         \r\n"+
                    "                                                                                         "
                );
            
                Console.WriteLine("Initializing...");

                Console.WriteLine("-Dependency...");
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                    // Load dependencies from C:\AliceInCradleHack\lib
                    string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                    string assemblyPath = Path.Combine("C:\\AliceInCradleHack\\lib", assemblyName);

                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                    return null;
                };
                Console.WriteLine("done");

                Console.WriteLine("-CommandManager...");
                commandManager.Initialize();
                Console.WriteLine("done");

                Console.WriteLine("-ModuleManager...");
                moduleManager.Initialize();
                Console.WriteLine("done");

                Console.WriteLine("Initialization complete.");

                Console.WriteLine("Injection successful!");
                Console.ResetColor();
                commandManager.RunCommandLoop();
            }
            catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Injection failed: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Please eject the DLL.");
                Console.ResetColor();
            }
        }

        // Actually ,it will be crashed if we use SharpInjector to eject the dll.
        // IDK why.
        static void Eject()
        {
            FreeConsole();
        }
    }
}
