using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AliceInCradleHack.script
{
    public sealed class LuaScriptContext : IDisposable
    {
        internal LuaScriptContext(string name, string path)
        {
            Name = name;
            Path = path;
            Script = new MoonSharp.Interpreter.Script(CoreModules.Preset_HardSandbox);
            Subscriptions = new List<Tuple<EventInfo, Delegate>>();
        }

        public string Name { get; }
        public string Path { get; }
        public MoonSharp.Interpreter.Script Script { get; }
        internal List<Tuple<EventInfo, Delegate>> Subscriptions { get; }

        public void Dispose()
        {
            Subscriptions.Clear();
        }
    }
}
