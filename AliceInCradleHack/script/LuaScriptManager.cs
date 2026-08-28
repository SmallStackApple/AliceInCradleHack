using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AliceInCradleHack.script
{
    public sealed class LuaScriptManager : IClientComponent
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, LuaScriptInfo> _scripts = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LuaScriptContext> _contexts = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object UserDataLock = new object();
        private static readonly HashSet<Type> RegisteredUserDataTypes = new();
        private bool _initialized;
        private string _scriptFolder;
        private Config _scriptConfig;
        private StringListValue _enabledValue;

        private static readonly Lazy<LuaScriptManager> _instance = new(() => new LuaScriptManager());
        public static LuaScriptManager Instance => _instance.Value;
        private LuaScriptManager() { }

        public void Initialize()
        {
            if (_initialized) return;
            _scriptFolder = Path.Combine(MainFolder.GetMainFolder(), "Script");
            Directory.CreateDirectory(_scriptFolder);

            _scriptConfig = ConfigSystem.Root(new Config("Scripts", "Lua script enabled states"));
            _enabledValue = _scriptConfig.List("Enabled", null, "Scripts that are loaded automatically");
            bool firstRun = !File.Exists(_scriptConfig.JsonFile);
            ConfigSystem.Load(_scriptConfig);

            Scan();
            PruneMissingScripts();
            if (firstRun)
            {
                // Legacy behavior: without a saved state every script is loaded and recorded as enabled.
                foreach (var info in GetScripts())
                {
                    if (LoadScriptInternal(info.Name, false))
                        SetRecordedEnabled(info.Name, true, store: false);
                }
                ConfigSystem.Store(_scriptConfig);
            }
            else
            {
                foreach (string name in EnabledSnapshot())
                    LoadScriptInternal(name, false);
            }
            _initialized = true;
        }

        public void Dispose()
        {
            foreach (var name in _contexts.Keys.ToArray()) UnloadInternal(name);
            _initialized = false;
        }

        public IEnumerable<LuaScriptInfo> GetScripts()
        {
            lock (_lock) return _scripts.Values.Select(Copy).OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        /// <summary>
        /// Registers newly appeared script files. Does not load anything.
        /// </summary>
        public void Scan()
        {
            ScanFilesOnly();
        }

        public bool LoadScript(string name)
        {
            if (!LoadScriptInternal(name, false)) return false;
            SetRecordedEnabled(name, true);
            return true;
        }

        public bool ReloadScript(string name) { UnloadInternal(name); return LoadScriptInternal(name, true); }

        public bool UnloadScript(string name)
        {
            if (!UnloadInternal(name)) return false;
            SetRecordedEnabled(name, false);
            return true;
        }

        /// <summary>
        /// Whether the script is recorded to load automatically on startup.
        /// </summary>
        public bool IsScriptEnabled(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || _enabledValue == null) return false;
            return _enabledValue.Items.Any(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
        }

        public void ReloadAll()
        {
            ScanFilesOnly();
            foreach (var name in _contexts.Keys.ToArray()) UnloadInternal(name);
            foreach (string name in EnabledSnapshot()) LoadScriptInternal(name, false);
        }

        /// <summary>
        /// Unloads a script without touching the persisted enabled state.
        /// Used by Dispose/Reload paths where the recorded state must survive.
        /// </summary>
        private bool UnloadInternal(string name)
        {
            LuaScriptContext context;
            lock (_lock) if (!_contexts.TryGetValue(name, out context)) return false;
            try { CallLifecycle(context, "OnUnload"); } catch (Exception ex) { Log.Error($"Lua OnUnload failed: {name}", ex); }
            LuaHookManager.RemoveAll(context);
            UnsubscribeAll(context);
            context.Dispose();
            lock (_lock) _contexts.Remove(name);
            lock (_lock) if (_scripts.TryGetValue(name, out var info)) info.IsLoaded = false;
            return true;
        }

        private string[] EnabledSnapshot()
        {
            return _enabledValue?.Items.ToArray() ?? Array.Empty<string>();
        }

        private void SetRecordedEnabled(string name, bool enabled, bool store = true)
        {
            if (_enabledValue == null || string.IsNullOrWhiteSpace(name)) return;
            var items = _enabledValue.Items.ToList();
            int index = items.FindIndex(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
            if (enabled && index < 0) items.Add(name);
            else if (!enabled && index >= 0) items.RemoveAt(index);
            else
            {
                lock (_lock) if (_scripts.TryGetValue(name, out var unchanged)) unchanged.IsEnabled = enabled;
                return;
            }
            _enabledValue.Set(items);
            lock (_lock) if (_scripts.TryGetValue(name, out var info)) info.IsEnabled = enabled;
            if (store) ConfigSystem.Store(_scriptConfig);
        }

        /// <summary>
        /// Drops recorded entries whose script file no longer exists.
        /// </summary>
        private void PruneMissingScripts()
        {
            if (_enabledValue == null) return;
            var items = _enabledValue.Items.ToList();
            int removed = items.RemoveAll(item =>
            {
                lock (_lock) return !_scripts.ContainsKey(item);
            });
            if (removed > 0) _enabledValue.Set(items);
        }

        private bool LoadScriptInternal(string name, bool known)
        {
            if (string.IsNullOrWhiteSpace(name) || Path.GetFileName(name) != name || !name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase)) return false;
            string path = Path.Combine(_scriptFolder, name);
            if (!File.Exists(path)) return false;
            lock (_lock) if (_contexts.ContainsKey(name)) return true;
            LuaScriptInfo info;
            lock (_lock)
            {
                if (!_scripts.TryGetValue(name, out info))
                {
                    info = new LuaScriptInfo { Name = name, Path = path };
                    _scripts[name] = info;
                }
                info.IsEnabled = IsScriptEnabled(name);
            }
            var context = new LuaScriptContext(name, path);
            try
            {
                LuaApi.Register(context);
                context.Script.DoString(File.ReadAllText(path), null, name);
                CallLifecycle(context, "OnLoad");
                lock (_lock) { _contexts[name] = context; info.IsLoaded = true; info.LoadedAt = DateTime.Now; info.Error = null; }
                Log.Info($"Loaded Lua script: {name}");
                return true;
            }
            catch (Exception ex)
            {
                UnsubscribeAll(context); context.Dispose();
                info.Error = ex.ToString(); info.IsLoaded = false;
                Log.Error($"Failed to load Lua script: {name}", ex);
                return false;
            }
        }

        private void ScanFilesOnly()
        {
            Directory.CreateDirectory(_scriptFolder);
            foreach (var file in Directory.GetFiles(_scriptFolder, "*.lua"))
                lock (_lock) if (!_scripts.ContainsKey(Path.GetFileName(file))) _scripts[Path.GetFileName(file)] = new LuaScriptInfo { Name = Path.GetFileName(file), Path = file };
        }

        private static void CallLifecycle(LuaScriptContext context, string name)
        {
            var value = context.Script.Globals.Get(name);
            if (value.Type == DataType.Function) context.Script.Call(value);
        }

        internal static void Subscribe(LuaScriptContext context, CallbackArguments args)
        {
            string name = args.AsType(0, "eventName", DataType.String).String;
            var callback = args.AsType(1, "callback", DataType.Function);
            EventInfo eventInfo = FindEvent(name);
            if (eventInfo == null) throw new ArgumentException($"Event not found: {name}");
            var handler = CreateHandler(context, eventInfo, callback);
            eventInfo.AddEventHandler(null, handler);
            context.Subscriptions.Add(Tuple.Create(eventInfo, handler));
        }

        internal static void Unsubscribe(LuaScriptContext context, CallbackArguments args)
        {
            string name = args.AsType(0, "eventName", DataType.String).String;
            foreach (var item in context.Subscriptions.Where(x => x.Item1.Name == name).ToArray())
            {
                item.Item1.RemoveEventHandler(null, item.Item2); context.Subscriptions.Remove(item);
            }
        }

        internal static void UnsubscribeAll(LuaScriptContext context)
        {
            foreach (var item in context.Subscriptions.ToArray())
                try { item.Item1.RemoveEventHandler(null, item.Item2); } catch { }
            context.Subscriptions.Clear();
        }

        private static EventInfo FindEvent(string name) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => SafeTypes(a)).SelectMany(t => t.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)).FirstOrDefault(e => e.Name == name);
        private static IEnumerable<Type> SafeTypes(Assembly assembly) { try { return assembly.GetTypes(); } catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); } }

        private static Delegate CreateHandler(LuaScriptContext context, EventInfo eventInfo, DynValue callback)
        {
            var invoke = eventInfo.EventHandlerType.GetMethod("Invoke");
            var parameters = invoke.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            Expression sender = parameters.Length > 0 ? Expression.Convert(parameters[0], typeof(object)) : Expression.Constant(null, typeof(object));
            Expression eventArgs = parameters.Length > 1 ? Expression.Convert(parameters[1], typeof(object)) : Expression.Constant(null, typeof(object));
            var call = Expression.Call(typeof(LuaScriptManager), nameof(InvokeLua), null, Expression.Constant(context), Expression.Constant(callback), sender, eventArgs);
            return Expression.Lambda(eventInfo.EventHandlerType, call, parameters).Compile();
        }

        private static void InvokeLua(LuaScriptContext context, DynValue callback, object sender, object args)
        {
            try
            {
                context.Script.Call(callback, ToLuaValue(context.Script, sender), ToLuaValue(context.Script, args));
            }
            catch (Exception ex) { Log.Error($"Lua event callback failed: {context.Name}", ex); }
        }

        private static DynValue ToLuaValue(MoonSharp.Interpreter.Script script, object value)
        {
            if (value == null) return DynValue.Nil;

            Type type = value.GetType();
            lock (UserDataLock)
            {
                if (RegisteredUserDataTypes.Add(type))
                    UserData.RegisterType(type, InteropAccessMode.Reflection);
            }
            return DynValue.FromObject(script, value);
        }

        private static LuaScriptInfo Copy(LuaScriptInfo source) => new LuaScriptInfo { Name = source.Name, Path = source.Path, IsLoaded = source.IsLoaded, IsEnabled = source.IsEnabled, Error = source.Error, LoadedAt = source.LoadedAt };
    }
}
