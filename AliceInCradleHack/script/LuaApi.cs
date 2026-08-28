using AliceInCradleHack.module;
using AliceInCradleHack.utils.client;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace AliceInCradleHack.script
{
    internal static class LuaApi
    {
        public static void Register(LuaScriptContext context)
        {
            var script = context.Script;
            var log = new Table(script);
            log.Set("Debug", DynValue.NewCallback((c, a) => { Log.Debug(Message(a)); return DynValue.Nil; }));
            log.Set("Info", DynValue.NewCallback((c, a) => { Log.Info(Message(a)); return DynValue.Nil; }));
            log.Set("Warn", DynValue.NewCallback((c, a) => { Log.Warn(Message(a)); return DynValue.Nil; }));
            log.Set("Error", DynValue.NewCallback((c, a) => { Log.Error(Message(a)); return DynValue.Nil; }));
            script.Globals.Set("Log", DynValue.NewTable(log));

            var modules = new Table(script);
            modules.Set("EnableModule", DynValue.NewCallback((c, a) => { ModuleManager.Instance.EnableModule(a.AsType(0, "moduleName", DataType.String).String); return DynValue.Nil; }));
            modules.Set("DisableModule", DynValue.NewCallback((c, a) => { ModuleManager.Instance.DisableModule(a.AsType(0, "moduleName", DataType.String).String); return DynValue.Nil; }));
            modules.Set("ToggleModule", DynValue.NewCallback((c, a) => { ModuleManager.Instance.ToggleModule(a.AsType(0, "moduleName", DataType.String).String); return DynValue.Nil; }));
            modules.Set("IsModuleEnabled", DynValue.NewCallback((c, a) => DynValue.NewBoolean(ModuleManager.Instance.IsModuleEnabled(a.AsType(0, "moduleName", DataType.String).String))));
            modules.Set("GetModuleByName", DynValue.NewCallback((c, a) => DynValue.FromObject(script, ModuleManager.Instance.GetModuleByName(a.AsType(0, "moduleName", DataType.String).String))));
            modules.Set("GetAllModules", DynValue.NewCallback((c, a) => DynValue.FromObject(script, ModuleManager.Instance.GetAllModules().ToArray())));
            modules.Set("GetSettingValue", DynValue.NewCallback((c, a) => DynValue.FromObject(script, ModuleManager.Instance.GetSettingValue(a.AsType(0, "moduleName", DataType.String).String, a.AsType(1, "settingPath", DataType.String).String))));
            modules.Set("SetSettingValue", DynValue.NewCallback((c, a) => DynValue.NewBoolean(ModuleManager.Instance.SetSettingValue(a.AsType(0, "moduleName", DataType.String).String, a.AsType(1, "settingPath", DataType.String).String, a[2].ToObject()))));
            script.Globals.Set("ModuleManager", DynValue.NewTable(modules));

            var client = new Table(script);
            client.Set("ClientName", DynValue.NewString(Client.ClientName)); client.Set("VersionType", DynValue.NewString(Client.VersionType)); client.Set("Version", DynValue.NewString(Client.Version)); client.Set("GitHash", DynValue.NewString(Client.GitHash));
            script.Globals.Set("Client", DynValue.NewTable(client));
            var events = new Table(script);
            events.Set("On", DynValue.NewCallback((c, a) => { LuaScriptManager.Subscribe(context, a); return DynValue.Nil; }));
            events.Set("Off", DynValue.NewCallback((c, a) => { LuaScriptManager.Unsubscribe(context, a); return DynValue.Nil; }));
            events.Set("OffAll", DynValue.NewCallback((c, a) => { LuaScriptManager.UnsubscribeAll(context); return DynValue.Nil; }));
            script.Globals.Set("Event", DynValue.NewTable(events));

            var harmony = new Table(script);
            harmony.Set("Patch", DynValue.NewCallback((c, a) => DynValue.NewBoolean(LuaHookManager.PatchFromLua(context, a))));
            harmony.Set("Unpatch", DynValue.NewCallback((c, a) => DynValue.NewBoolean(LuaHookManager.UnpatchFromLua(context, a))));
            script.Globals.Set("Harmony", DynValue.NewTable(harmony));

            LuaGameApi.Register(context);
            LuaReflection.Register(script);
        }

        private static string Message(CallbackArguments args) => args.Count == 0 ? "" : args[0].ToObject()?.ToString() ?? "nil";
    }
}
