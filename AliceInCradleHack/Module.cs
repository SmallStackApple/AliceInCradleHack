using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack
{
    public abstract class Module
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public abstract bool IsEnabled { get; set; }
        public virtual string Category { get; } = "General";

        public abstract ArrayList Settings { get; set;}

        // Do not block these methods
        public abstract void Initialize();
        public abstract void Enable();
        public abstract void Disable();
    }

    public class ModuleManager
    {
        private readonly Dictionary<string, Module> Modules = new Dictionary<string, Module>();

        public readonly static ModuleManager Instance = new ModuleManager();

        public void Initialize()
        {
            List<Module> modules = new List<Module>
            {
                new Modules.ModuleMosaicRemove(),
                // Add other module instances here
            };
            foreach (var module in modules)
            {
                RegisterModule(module);
            }
        }

        public void RegisterModuleFromAssemblyFile(string assemblyPath)
        {
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);
                var moduleTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Module)) && !t.IsAbstract);
                foreach (var type in moduleTypes)
                {
                    var moduleInstance = (Module)Activator.CreateInstance(type);
                    RegisterModule(moduleInstance);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load modules from assembly {assemblyPath}: {ex.Message}");
            }
        }

        public IEnumerable<Module> GetAllModules()
        {
            return Modules.Values;
        }

        public void RegisterModule(Module module)
        {
            if (!Modules.ContainsKey(module.Name))
            {
                Modules.Add(module.Name, module);
                module.Initialize();
            }
        }

        public void EnableModule(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
            {
                var module = Modules[moduleName];
                if (!module.IsEnabled)
                {
                    module.Enable();
                    module.IsEnabled = true;
                }
            }
        }

        public void DisableModule(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
            {
                var module = Modules[moduleName];
                if (module.IsEnabled)
                {
                    module.Disable();
                    module.IsEnabled = false;
                }
            }
        }

        public void ToggleModule(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
            {
                var module = Modules[moduleName];
                if (module.IsEnabled)
                {
                    DisableModule(moduleName);
                }
                else
                {
                    EnableModule(moduleName);
                }
            }
        }

        public Module GetModuleByName(string moduleName)
        {
            Modules.TryGetValue(moduleName, out var module);
            return module;
        }

        public IEnumerable<Module> GetModulesByCategory(string category)
        {
            return Modules.Values.Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Module> GetEnabledModules()
        {
            return Modules.Values.Where(m => m.IsEnabled);
        }

        public IEnumerable<Module> GetDisabledModules()
        {
            return Modules.Values.Where(m => !m.IsEnabled);
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (Modules.ContainsKey(moduleName))
            {
                return Modules[moduleName].IsEnabled;
            }
            return false;
        }
    }
}
