# Lua Scripts

Lua files in `<mainFolder>\Script\` are managed from the WebUI Scripts page (or the console).
The enabled state of every script is persisted in `configs\scripts.json`: enabled scripts are
loaded automatically on startup, disabled ones stay unloaded. On the very first run all
discovered scripts are loaded and recorded as enabled; scripts added later start disabled.

```lua
function OnLoad()
    Log.Info(Client.ClientName .. " " .. Client.Version)
    Event.On("EventPostUpdate", function(sender, args)
        -- Event callbacks receive the original C# sender and EventArgs objects.
    end)
end

function OnUnload()
    Log.Info("Script unloaded")
end
```

Available globals preserve the C# naming convention:

- `Log.Debug`, `Log.Info`, `Log.Warn`, `Log.Error`
- `Client.ClientName`, `Client.VersionType`, `Client.Version`, `Client.GitHash`
- `ModuleManager.EnableModule`, `DisableModule`, `ToggleModule`, `IsModuleEnabled`,
  `GetModuleByName`, `GetAllModules`, `GetSettingValue`, `SetSettingValue`
- `Event.On`, `Event.Off`, `Event.OffAll`
- `Player.Exists`, `Player.GetHp`, `Player.GetMaxHp`, `Player.GetMp`, `Player.GetMaxMp`,
  `Player.GetInstance` (returns the `PRNoel` userdata or nil)
- `Game.GetM2DBase`, `Game.GetItemManager`, `Game.AddAlert(message[, alertType])`
- `Notification.Notify(message)` (Dynamic Island + in-game UILog)
- `Input.GetKey`, `GetKeyDown`, `GetKeyUp` (key name, `Unity.KeyCode.*` value or number),
  `Input.GetMouseButton`, `GetMouseButtonDown`, `GetMouseButtonUp` (button index),
  `Input.GetMousePosition()` -> `{ x =, y =, z = }`
- `Unity.Vector2(x, y)`, `Unity.Vector3(x, y[, z])`, `Unity.Vector4(x, y[, z[, w]])`,
  `Unity.Color(r, g, b[, a])`, `Unity.KeyCode` (name -> KeyCode userdata)
- `Harmony.Patch`, `Harmony.Unpatch`
- `Reflection.GetAssemblies`, `GetAssembly`, `GetType`, `GetMembers`, `GetMethods`,
  `GetFields`, `GetProperties`, `GetEvents`, `GetConstructors`, `GetMethod`, `GetField`,
  `GetProperty`, `GetEvent`, `CreateInstance`, `GetValue`, `SetValue`, `Invoke`

`Reflection` can inspect all assemblies already loaded into the current AppDomain, including
non-public static and instance members. It cannot load additional assemblies.

## Harmony hooks

`Harmony.Patch(method, hooks)` hooks any patchable method (including non-public ones) with
Lua callbacks. `method` is a `MethodInfo` from `Reflection.GetMethod`/`GetMethods`, or a
`"Full.Type.Name:MethodName"` string (first overload is used; prefer `MethodInfo` for
overloaded methods). All hooks of a script are removed automatically when it is unloaded.

```lua
Harmony.Patch("m2d.M2Attackable:applyHpDamage", {
    prefix = function(ctx)
        -- ctx.instance       the target instance (nil for static methods)
        -- ctx.args           1-based array of the call arguments; writes change the call
        -- ctx.result         set it to override the return value (non-void methods)
        -- ctx.originalMethod the hooked MethodInfo
        ctx.args[1] = 0        -- zero out the damage value
        -- return false        -- would skip the original method entirely
    end,
    postfix = function(ctx)
        -- ctx.result holds the current return value and can be reassigned
    end
})
```

Notes:

- Only prefix/postfix are supported (no transpilers).
- Multiple `Patch` calls on the same method stack; `Harmony.Unpatch(method)` removes all
  hooks of the calling script on that method.
- Exceptions inside callbacks are logged and never crash the game.
- `ctx.args` write-back converts values to the declared parameter types (numbers, strings,
  enums by name, userdata objects).
