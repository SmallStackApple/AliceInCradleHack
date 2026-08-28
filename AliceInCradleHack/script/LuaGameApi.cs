using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using MoonSharp.Interpreter;
using System;
using UnityEngine;

namespace AliceInCradleHack.script
{
    /// <summary>
    /// Convenience Lua globals for interacting with the game: player stats, game
    /// singletons, Unity input, notifications and common Unity value types.
    /// </summary>
    internal static class LuaGameApi
    {
        private static bool _unityTypesRegistered;
        private static readonly object _registerLock = new();

        public static void Register(LuaScriptContext context)
        {
            var script = context.Script;
            RegisterUnityTypes();

            // Player: live stats of the player character (Noel). -1 while no player exists.
            var player = new Table(script);
            player.Set("Exists", DynValue.NewCallback((c, a) => DynValue.NewBoolean(Player.Instance != null)));
            player.Set("GetHp", DynValue.NewCallback((c, a) => DynValue.NewNumber(Player.Hp)));
            player.Set("GetMaxHp", DynValue.NewCallback((c, a) => DynValue.NewNumber(Player.MaxHp)));
            player.Set("GetMp", DynValue.NewCallback((c, a) => DynValue.NewNumber(Player.Mp)));
            player.Set("GetMaxMp", DynValue.NewCallback((c, a) => DynValue.NewNumber(Player.MaxMp)));
            player.Set("GetInstance", DynValue.NewCallback((c, a) => DynValue.FromObject(script, Player.Instance)));
            script.Globals.Set("Player", DynValue.NewTable(player));

            // Game: game singletons and in-game alerts.
            var game = new Table(script);
            game.Set("GetM2DBase", DynValue.NewCallback((c, a) => DynValue.FromObject(script, NelM2DBase.Instance)));
            game.Set("GetItemManager", DynValue.NewCallback((c, a) => DynValue.FromObject(script, NelItemManager.Instance)));
            game.Set("AddAlert", DynValue.NewCallback((c, a) => AddAlert(a)));
            script.Globals.Set("Game", DynValue.NewTable(game));

            // Notification: Dynamic Island + in-game UILog.
            var notification = new Table(script);
            notification.Set("Notify", DynValue.NewCallback((c, a) =>
            {
                string message = a.Count == 0 ? "" : a[0].ToObject()?.ToString() ?? "nil";
                Notification.ShowNotificationByUILog(message);
                Notification.ShowNotificationByDynamicIsland(message);
                return DynValue.Nil;
            }));
            script.Globals.Set("Notification", DynValue.NewTable(notification));

            // Input: UnityEngine.Input. Keys accept names ("Space", "F1", "Alpha1"),
            // KeyCode userdata (Unity.KeyCode.Space) or numeric key codes.
            var input = new Table(script);
            input.Set("GetKey", DynValue.NewCallback((c, a) => KeyQuery(a, Input.GetKey)));
            input.Set("GetKeyDown", DynValue.NewCallback((c, a) => KeyQuery(a, Input.GetKeyDown)));
            input.Set("GetKeyUp", DynValue.NewCallback((c, a) => KeyQuery(a, Input.GetKeyUp)));
            input.Set("GetMouseButton", DynValue.NewCallback((c, a) => MouseQuery(a, Input.GetMouseButton)));
            input.Set("GetMouseButtonDown", DynValue.NewCallback((c, a) => MouseQuery(a, Input.GetMouseButtonDown)));
            input.Set("GetMouseButtonUp", DynValue.NewCallback((c, a) => MouseQuery(a, Input.GetMouseButtonUp)));
            input.Set("GetMousePosition", DynValue.NewCallback((c, a) =>
            {
                try
                {
                    var pos = Input.mousePosition;
                    var t = new Table(script);
                    t.Set("x", DynValue.NewNumber(pos.x));
                    t.Set("y", DynValue.NewNumber(pos.y));
                    t.Set("z", DynValue.NewNumber(pos.z));
                    return DynValue.NewTable(t);
                }
                catch
                {
                    return DynValue.Nil;
                }
            }));
            script.Globals.Set("Input", DynValue.NewTable(input));

            // Unity: constructors for common value types and the KeyCode enum table.
            var unity = new Table(script);
            unity.Set("Vector2", DynValue.NewCallback((c, a) => DynValue.FromObject(script, new Vector2((float)a[0].Number, (float)a[1].Number))));
            unity.Set("Vector3", DynValue.NewCallback((c, a) => DynValue.FromObject(script, new Vector3((float)a[0].Number, (float)a[1].Number, a.Count > 2 ? (float)a[2].Number : 0f))));
            unity.Set("Vector4", DynValue.NewCallback((c, a) => DynValue.FromObject(script, new Vector4((float)a[0].Number, (float)a[1].Number, a.Count > 2 ? (float)a[2].Number : 0f, a.Count > 3 ? (float)a[3].Number : 0f))));
            unity.Set("Color", DynValue.NewCallback((c, a) => DynValue.FromObject(script, new Color((float)a[0].Number, (float)a[1].Number, (float)a[2].Number, a.Count > 3 ? (float)a[3].Number : 1f))));
            var keyCodes = new Table(script);
            foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
                keyCodes.Set(value.ToString(), DynValue.FromObject(script, value));
            unity.Set("KeyCode", DynValue.NewTable(keyCodes));
            script.Globals.Set("Unity", DynValue.NewTable(unity));
        }

        private static DynValue AddAlert(CallbackArguments args)
        {
            try
            {
                string message = args.Count == 0 ? "" : args[0].ToObject()?.ToString() ?? "nil";
                var type = nel.UILogRow.TYPE.ALERT;
                if (args.Count > 1 && args[1].Type == DataType.String)
                    Enum.TryParse(args[1].String, true, out type);
                UILog.AddAlert(message, type);
            }
            catch (Exception ex)
            {
                Log.Error("Lua Game.AddAlert failed", ex);
            }
            return DynValue.Nil;
        }

        private static DynValue KeyQuery(CallbackArguments args, Func<KeyCode, bool> query)
        {
            if (!TryGetKey(args, out var key)) return DynValue.False;
            try
            {
                return DynValue.NewBoolean(query(key));
            }
            catch
            {
                return DynValue.False;
            }
        }

        private static DynValue MouseQuery(CallbackArguments args, Func<int, bool> query)
        {
            int button = args.Count > 0 && args[0].Type == DataType.Number ? (int)args[0].Number : 0;
            try
            {
                return DynValue.NewBoolean(query(button));
            }
            catch
            {
                return DynValue.False;
            }
        }

        private static bool TryGetKey(CallbackArguments args, out KeyCode key)
        {
            key = KeyCode.None;
            if (args.Count == 0) return false;
            var value = args[0];
            switch (value.Type)
            {
                case DataType.String:
                    return Enum.TryParse(value.String, true, out key) && Enum.IsDefined(typeof(KeyCode), key);
                case DataType.Number:
                    key = (KeyCode)(int)value.Number;
                    return Enum.IsDefined(typeof(KeyCode), key);
                case DataType.UserData when value.UserData?.Object is KeyCode code:
                    key = code;
                    return true;
                default:
                    return false;
            }
        }

        private static void RegisterUnityTypes()
        {
            lock (_registerLock)
            {
                if (_unityTypesRegistered) return;
                _unityTypesRegistered = true;
                UserData.RegisterType(typeof(Vector2), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(Vector3), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(Vector4), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(Color), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(Color32), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(Quaternion), InteropAccessMode.Reflection);
                UserData.RegisterType(typeof(KeyCode), InteropAccessMode.Reflection);
            }
        }
    }
}
