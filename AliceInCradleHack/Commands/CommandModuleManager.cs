using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Commands
{
    public class CommandModuleManager : Command
    {
        public override string Name => "module";
        public override string Description => "Module manager command.";
        public override string Usage =>
            "module [subcommands]\n" +
            "list - List all modules\n" +
            "help - Show this help message";
        private ModuleManager ModuleManager => ModuleManager.Instance;
        private readonly Dictionary<string, Action<string[]>> SubCommands;

        public CommandModuleManager()
        {
            SubCommands = new Dictionary<string, Action<string[]>>
            {
                { "list", args => ListModules() },
                { "help", args => GetHelp() },
                { "toggle", args => ToggleModule(args) }
            };
        }

        public override void Execute(string[] args)
        {
            if (args.Length == 0 || !SubCommands.ContainsKey(args[0]))
            {
                GetHelp();
                return;
            }
            SubCommands[args[0]](args.Skip(1).ToArray());
        }

        private void GetHelp()
        {
            Console.WriteLine(Usage);
        }

        private void ListModules()
        {
            Console.WriteLine("Available Modules:");
            foreach (var module in ModuleManager.GetAllModules())
            {
                Console.WriteLine($"{module.Name} - {module.Description} - Enabled: {module.IsEnabled}");
            }
        }

        private void ToggleModule(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: module toggle [module_name]");
                return;
            }
            string moduleName = args[0];
            var module = ModuleManager.GetAllModules().FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found.");
                return;
            }
            Console.WriteLine($"Module '{module.Name}' is now {(module.IsEnabled ? "enabled" : "disabled")}.");
        }
    }
}
