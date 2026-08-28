using AliceInCradleHack.utils.client;
using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AliceInCradleHack.script
{
    /// <summary>
    /// Bridges Lua callbacks to Harmony patches. A single shared Harmony instance patches
    /// each target method at most once per hook kind (prefix/postfix); the static dispatchers
    /// look up the registered Lua callbacks by <c>__originalMethod</c> and invoke them with a
    /// mutable context table (instance / args / result / originalMethod).
    /// All hooks of a script are removed automatically when the script is unloaded.
    /// </summary>
    internal static class LuaHookManager
    {
        private sealed class HookEntry
        {
            public MethodInfo Original;
            public bool IsVoid;
            public Type[] ParameterTypes;
            public bool PrefixPatched;
            public bool PostfixPatched;
            public readonly List<KeyValuePair<LuaScriptContext, DynValue>> PrefixHandlers = new();
            public readonly List<KeyValuePair<LuaScriptContext, DynValue>> PostfixHandlers = new();
        }

        private static readonly object _lock = new();
        private static readonly Dictionary<MethodInfo, HookEntry> _hooks = new();
        private static readonly Harmony _harmony = new(Client.ClientName.ToLowerInvariant() + ".lua");

        private static readonly MethodInfo PrefixVoidDispatcher = typeof(LuaHookManager).GetMethod(nameof(DispatchPrefixVoid), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo PrefixResultDispatcher = typeof(LuaHookManager).GetMethod(nameof(DispatchPrefixResult), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo PostfixVoidDispatcher = typeof(LuaHookManager).GetMethod(nameof(DispatchPostfixVoid), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo PostfixResultDispatcher = typeof(LuaHookManager).GetMethod(nameof(DispatchPostfixResult), BindingFlags.NonPublic | BindingFlags.Static);

        // Lua-facing entry points

        public static bool PatchFromLua(LuaScriptContext context, CallbackArguments args)
        {
            var method = ResolveMethod(args.Count > 0 ? args[0] : DynValue.Nil);
            if (method == null) throw new ArgumentException("Unknown method. Pass a MethodInfo (Reflection.GetMethod) or 'Full.Type.Name:MethodName'.");
            if (args.Count < 2 || args[1].Type != DataType.Table) throw new ArgumentException("Missing hooks table: { prefix = fn, postfix = fn }");
            var hooks = args[1].Table;
            var prefix = hooks.Get("prefix");
            var postfix = hooks.Get("postfix");
            if (prefix.Type != DataType.Function && postfix.Type != DataType.Function)
                throw new ArgumentException("Hooks table needs at least one of 'prefix' / 'postfix' functions.");
            return Patch(context, method,
                prefix.Type == DataType.Function ? prefix : null,
                postfix.Type == DataType.Function ? postfix : null);
        }

        public static bool UnpatchFromLua(LuaScriptContext context, CallbackArguments args)
        {
            var method = ResolveMethod(args.Count > 0 ? args[0] : DynValue.Nil);
            if (method == null) throw new ArgumentException("Unknown method. Pass a MethodInfo (Reflection.GetMethod) or 'Full.Type.Name:MethodName'.");
            return Unpatch(context, method);
        }

        // Hook management

        public static bool Patch(LuaScriptContext context, MethodInfo original, DynValue prefix, DynValue postfix)
        {
            if (context == null || original == null || (prefix == null && postfix == null)) return false;
            if (!IsPatchable(original)) return false;
            lock (_lock)
            {
                if (!_hooks.TryGetValue(original, out var entry))
                {
                    entry = new HookEntry
                    {
                        Original = original,
                        IsVoid = original.ReturnType == typeof(void),
                        ParameterTypes = original.GetParameters()
                            .Select(p => p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType)
                            .ToArray()
                    };
                    _hooks[original] = entry;
                }
                if (prefix != null) entry.PrefixHandlers.Add(new KeyValuePair<LuaScriptContext, DynValue>(context, prefix));
                if (postfix != null) entry.PostfixHandlers.Add(new KeyValuePair<LuaScriptContext, DynValue>(context, postfix));
                ApplyLocked(entry);
            }
            return true;
        }

        public static bool Unpatch(LuaScriptContext context, MethodInfo original)
        {
            if (context == null || original == null) return false;
            lock (_lock)
            {
                if (!_hooks.TryGetValue(original, out var entry)) return false;
                bool removed = entry.PrefixHandlers.RemoveAll(p => p.Key == context) +
                               entry.PostfixHandlers.RemoveAll(p => p.Key == context) > 0;
                CleanupLocked(entry);
                return removed;
            }
        }

        /// <summary>
        /// Removes every hook registered by the given script (called on unload/reload/shutdown).
        /// </summary>
        public static void RemoveAll(LuaScriptContext context)
        {
            if (context == null) return;
            lock (_lock)
            {
                foreach (var entry in _hooks.Values.ToArray())
                {
                    entry.PrefixHandlers.RemoveAll(p => p.Key == context);
                    entry.PostfixHandlers.RemoveAll(p => p.Key == context);
                    CleanupLocked(entry);
                }
            }
        }

        private static void ApplyLocked(HookEntry entry)
        {
            try
            {
                if (entry.PrefixHandlers.Count > 0 && !entry.PrefixPatched)
                {
                    _harmony.Patch(entry.Original, prefix: new HarmonyMethod(entry.IsVoid ? PrefixVoidDispatcher : PrefixResultDispatcher));
                    entry.PrefixPatched = true;
                    Log.Info($"Lua Harmony prefix patched: {Describe(entry.Original)}");
                }
                if (entry.PostfixHandlers.Count > 0 && !entry.PostfixPatched)
                {
                    _harmony.Patch(entry.Original, postfix: new HarmonyMethod(entry.IsVoid ? PostfixVoidDispatcher : PostfixResultDispatcher));
                    entry.PostfixPatched = true;
                    Log.Info($"Lua Harmony postfix patched: {Describe(entry.Original)}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Lua Harmony patch failed: {Describe(entry.Original)}", ex);
            }
        }

        /// <summary>
        /// Unpatches hook kinds with no remaining handlers and drops fully empty entries.
        /// Must be called under _lock.
        /// </summary>
        private static void CleanupLocked(HookEntry entry)
        {
            try
            {
                if (entry.PrefixPatched && entry.PrefixHandlers.Count == 0)
                {
                    _harmony.Unpatch(entry.Original, HarmonyPatchType.Prefix, _harmony.Id);
                    entry.PrefixPatched = false;
                }
                if (entry.PostfixPatched && entry.PostfixHandlers.Count == 0)
                {
                    _harmony.Unpatch(entry.Original, HarmonyPatchType.Postfix, _harmony.Id);
                    entry.PostfixPatched = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Lua Harmony unpatch failed: {Describe(entry.Original)}", ex);
            }
            if (entry.PrefixHandlers.Count == 0 && entry.PostfixHandlers.Count == 0)
                _hooks.Remove(entry.Original);
        }

        private static bool IsPatchable(MethodInfo method)
        {
            if (method.IsAbstract || method.ContainsGenericParameters)
            {
                Log.Warn($"Lua Harmony: method is not patchable: {Describe(method)}");
                return false;
            }
            return true;
        }

        // Harmony dispatchers (must be static; __originalMethod identifies the hook entry)

        private static bool DispatchPrefixVoid(object __instance, object[] __args, MethodBase __originalMethod)
            => DispatchPrefix(__originalMethod as MethodInfo, __instance, __args, null, out _);

        private static bool DispatchPrefixResult(object __instance, object[] __args, MethodBase __originalMethod, ref object __result)
        {
            bool run = DispatchPrefix(__originalMethod as MethodInfo, __instance, __args, __result, out object newResult);
            __result = newResult;
            return run;
        }

        private static void DispatchPostfixVoid(object __instance, object[] __args, MethodBase __originalMethod)
            => DispatchPostfix(__originalMethod as MethodInfo, __instance, __args, null, out _);

        private static void DispatchPostfixResult(object __instance, object[] __args, MethodBase __originalMethod, ref object __result)
        {
            DispatchPostfix(__originalMethod as MethodInfo, __instance, __args, __result, out object newResult);
            __result = newResult;
        }

        private static bool DispatchPrefix(MethodInfo original, object instance, object[] methodArgs, object result, out object newResult)
        {
            newResult = result;
            var handlers = GetHandlers(original, prefix: true);
            if (handlers == null) return true;
            bool runOriginal = true;
            foreach (var handler in handlers)
            {
                try
                {
                    var ret = InvokeHandler(handler.Key, handler.Value, original, instance, methodArgs, newResult, out var resultValue);
                    if (!resultValue.IsNil()) newResult = ConvertTo(resultValue, original.ReturnType);
                    if (ret.Type == DataType.Boolean && !ret.Boolean) runOriginal = false;
                }
                catch (Exception ex)
                {
                    Log.Error($"Lua harmony prefix failed: {handler.Key.Name}", ex);
                }
            }
            return runOriginal;
        }

        private static void DispatchPostfix(MethodInfo original, object instance, object[] methodArgs, object result, out object newResult)
        {
            newResult = result;
            var handlers = GetHandlers(original, prefix: false);
            if (handlers == null) return;
            foreach (var handler in handlers)
            {
                try
                {
                    InvokeHandler(handler.Key, handler.Value, original, instance, methodArgs, newResult, out var resultValue);
                    if (!resultValue.IsNil()) newResult = ConvertTo(resultValue, original.ReturnType);
                }
                catch (Exception ex)
                {
                    Log.Error($"Lua harmony postfix failed: {handler.Key.Name}", ex);
                }
            }
        }

        private static KeyValuePair<LuaScriptContext, DynValue>[] GetHandlers(MethodInfo original, bool prefix)
        {
            if (original == null) return null;
            lock (_lock)
            {
                if (!_hooks.TryGetValue(original, out var entry)) return null;
                var list = prefix ? entry.PrefixHandlers : entry.PostfixHandlers;
                return list.Count == 0 ? null : list.ToArray();
            }
        }

        /// <summary>
        /// Builds the Lua context table, calls the callback, then writes mutated args back
        /// into the Harmony argument array. The callback-visible 'result' field is exported
        /// via <paramref name="resultValue"/> (nil when untouched).
        /// </summary>
        private static DynValue InvokeHandler(LuaScriptContext context, DynValue callback, MethodInfo original,
            object instance, object[] methodArgs, object result, out DynValue resultValue)
        {
            var script = context.Script;
            var parameterTypes = GetParameterTypes(original);

            var argsTable = new Table(script);
            for (int i = 0; i < methodArgs.Length; i++)
                argsTable.Set(i + 1, DynValue.FromObject(script, methodArgs[i]));

            var ctxTable = new Table(script);
            ctxTable.Set("instance", DynValue.FromObject(script, instance));
            ctxTable.Set("args", DynValue.NewTable(argsTable));
            ctxTable.Set("result", result == null ? DynValue.Nil : DynValue.FromObject(script, result));
            ctxTable.Set("originalMethod", DynValue.FromObject(script, original));

            var ret = script.Call(callback, DynValue.NewTable(ctxTable));

            for (int i = 0; i < methodArgs.Length && i < parameterTypes.Length; i++)
            {
                var value = argsTable.Get(i + 1);
                if (value.IsNil()) continue;
                try
                {
                    methodArgs[i] = ConvertTo(value, parameterTypes[i]);
                }
                catch (Exception ex)
                {
                    Log.Error($"Lua harmony: failed to convert arg {i + 1} of {original.Name}", ex);
                }
            }

            resultValue = ctxTable.Get("result");
            return ret;
        }

        private static Type[] GetParameterTypes(MethodInfo original)
        {
            lock (_lock)
            {
                if (_hooks.TryGetValue(original, out var entry)) return entry.ParameterTypes;
            }
            return original.GetParameters()
                .Select(p => p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType)
                .ToArray();
        }

        // Value conversion

        private static object ConvertTo(DynValue value, Type targetType)
        {
            if (targetType == null || targetType == typeof(void)) return null;
            if (value == null || value.IsNil())
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            object obj = value.ToObject();
            if (obj == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            if (targetType.IsInstanceOfType(obj)) return obj;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlying.IsEnum)
            {
                if (obj is string text) return Enum.Parse(underlying, text, true);
                return Enum.ToObject(underlying, Convert.ChangeType(obj, Enum.GetUnderlyingType(underlying)));
            }
            return Convert.ChangeType(obj, underlying);
        }

        // Method resolution

        /// <summary>
        /// Resolves a Lua value to a MethodInfo: either a MethodInfo userdata
        /// (from Reflection.GetMethod) or a 'Full.Type.Name:MethodName' string.
        /// </summary>
        internal static MethodInfo ResolveMethod(DynValue value)
        {
            if (value == null || value.IsNil()) return null;
            if (value.Type == DataType.UserData && value.UserData?.Object is MethodInfo info) return info;
            if (value.Type != DataType.String) return null;

            string spec = value.String;
            int sep = spec.LastIndexOf(':');
            if (sep <= 0 || sep == spec.Length - 1) return null;
            string typeName = spec.Substring(0, sep);
            string methodName = spec.Substring(sep + 1);

            var type = ResolveType(typeName);
            if (type == null) return null;
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == methodName).ToArray();
            if (methods.Length > 1)
                Log.Warn($"Lua Harmony: '{spec}' has {methods.Length} overloads, using the first. Pass a MethodInfo from Reflection.GetMethods to pick one explicitly.");
            return methods.FirstOrDefault();
        }

        private static Type ResolveType(string name)
        {
            var type = Type.GetType(name);
            if (type != null) return type;
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(name, false))
                .FirstOrDefault(t => t != null);
        }

        private static string Describe(MethodBase method)
        {
            return method == null ? "(null)" : $"{method.DeclaringType?.FullName}.{method.Name}";
        }
    }
}
