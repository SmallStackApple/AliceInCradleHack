# Lua Scripts

Lua files in `<mainFolder>\Script\` are loaded automatically when the client initializes.

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
- `Reflection.GetAssemblies`, `GetAssembly`, `GetType`, `GetMembers`, `GetMethods`,
  `GetFields`, `GetProperties`, `GetEvents`, `GetConstructors`, `GetMethod`, `GetField`,
  `GetProperty`, `GetEvent`, `CreateInstance`, `GetValue`, `SetValue`, `Invoke`

`Reflection` can inspect all assemblies already loaded into the current AppDomain, including
non-public static and instance members. It cannot load additional assemblies.
