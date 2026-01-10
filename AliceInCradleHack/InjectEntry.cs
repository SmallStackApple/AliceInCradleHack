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
            // redirect console output to the new console
            Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.WriteLine("           _ _          _____        _____               _ _      _    _            _    \r\n     /\\   | (_)        |_   _|      / ____|             | | |    | |  | |          | |   \r\n    /  \\  | |_  ___ ___  | |  _ __ | |     _ __ __ _  __| | | ___| |__| | __ _  ___| | __\r\n   / /\\ \\ | | |/ __/ _ \\ | | | '_ \\| |    | '__/ _` |/ _` | |/ _ \\  __  |/ _` |/ __| |/ /\r\n  / ____ \\| | | (_|  __/_| |_| | | | |____| | | (_| | (_| | |  __/ |  | | (_| | (__|   < \r\n /_/    \\_\\_|_|\\___\\___|_____|_| |_|\\_____|_|  \\__,_|\\__,_|_|\\___|_|  |_|\\__,_|\\___|_|\\_\\\r\n                                                                                         \r\n                                                                                         ");
            Console.WriteLine("Hello world!");
        }

        static void Eject()
        {
            FreeConsole();
        }
    }
}
