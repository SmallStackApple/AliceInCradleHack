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
            "help - Show this help message\n"+
            "toggle [module_name] - Toggle a module on or off\n"+
            "info [module_name] - Show information about a module\n"+
            "set [module_name] [setting_key] [setting_value] - Set a module's setting\n"+
            "listsettings [module_name] - List all settings of a module";

        private ModuleManager ModuleManager => ModuleManager.Instance;
        private readonly Dictionary<string, Action<string[]>> SubCommands;

        public CommandModuleManager()
        {
            SubCommands = new Dictionary<string, Action<string[]>>
            {
                { "list", args => ListModules() },
                { "help", args => GetHelp() },
                { "toggle", args => ToggleModule(args) },
                { "info", args => ModuleInfo(args.Length > 0 ? args[0] : null) },
                { "set", args => SetModuleSetting(args) },
                { "listsettings", args => ListModuleSettings(args.Length > 0 ? args[0] : null) },
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
                Console.WriteLine($"{module.Name} - {module.Category} - {module.Description} - Enabled: {module.IsEnabled}");
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
            var module = ModuleManager.GetModuleByName(moduleName);
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found.");
                return;
            }
            ModuleManager.ToggleModule(module.Name);
            Console.WriteLine($"Module '{module.Name}' is now {(module.IsEnabled ? "enabled" : "disabled")}.");
        }

        private void ModuleInfo(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                Console.WriteLine("Usage: module info [module_name]");
                return;
            }
            var module = ModuleManager.GetAllModules().FirstOrDefault(m => m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found.");
                return;
            }
            Console.WriteLine($"Name: {module.Name}");
            Console.WriteLine($"Category: {module.Category}");
            Console.WriteLine($"Description: {module.Description}");
            Console.WriteLine($"Author: {module.Author}");
            Console.WriteLine($"Version: {module.Version}");
            Console.WriteLine($"Enabled: {module.IsEnabled}");
        }

        private void SetModuleSetting(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: module set [module_name] [setting_key] [setting_value]");
                return;
            }
            string moduleName = args[0];
            string settingKey = args[1];
            string settingValue = string.Join(" ", args.Skip(2));
            var module = ModuleManager.GetModuleByName(moduleName);
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found.");
                return;
            }
            if(module.Settings.GetNodeByPath(settingKey) == null)
            {
                Console.WriteLine($"Setting '{settingKey}' not found in module '{moduleName}'.");
                return;
            }
            module.Settings.SetValueByPath(settingKey, settingValue);
            Console.WriteLine($"Setting '{settingKey}' of module '{moduleName}' set to '{settingValue}'.");
        }

        private void ListModuleSettings(string moduleName)
        {
            var module = ModuleManager.GetModuleByName(moduleName);
            if (module == null)
            {
                Console.WriteLine($"Module '{moduleName}' not found.");
                return;
            }
            module.Settings.GetAllLeafNodes().ForEach(node => {
                Console.WriteLine($"{node.GetPath()} : {Convert.ToString(node.Value)} - {node.Description}");
            });
        }
    }
}
