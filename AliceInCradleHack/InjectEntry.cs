using System;
using System.Runtime.InteropServices;

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
            AllocConsole();
            //redirect input and output
            Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput()));
            //cancel ctrl+c handling to avoid terminating the host process
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
            };
            Console.WriteLine("           _ _          _____        _____               _ _      _    _            _    \r\n     /\\   | (_)        |_   _|      / ____|             | | |    | |  | |          | |   \r\n    /  \\  | |_  ___ ___  | |  _ __ | |     _ __ __ _  __| | | ___| |__| | __ _  ___| | __\r\n   / /\\ \\ | | |/ __/ _ \\ | | | '_ \\| |    | '__/ _` |/ _` | |/ _ \\  __  |/ _` |/ __| |/ /\r\n  / ____ \\| | | (_|  __/_| |_| | | | |____| | | (_| | (_| | |  __/ |  | | (_| | (__|   < \r\n /_/    \\_\\_|_|\\___\\___|_____|_| |_|\\_____|_|  \\__,_|\\__,_|_|\\___|_|  |_|\\__,_|\\___|_|\\_\\\r\n                                                                                         \r\n                                                                                         ");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Initializing...");

            Console.WriteLine("-CommandManager...");
            CommandManager commandManager = CommandManager.Instance;
            commandManager.Initialize();
            Console.WriteLine("done");

            Console.WriteLine("Initialization complete.");
            Console.ResetColor();

            commandManager.RunCommandLoop();
        }

        static void Eject()
        {
            FreeConsole();
        }
    }
}
