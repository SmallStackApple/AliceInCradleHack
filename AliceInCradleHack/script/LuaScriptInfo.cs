using System;

namespace AliceInCradleHack.script
{
    public sealed class LuaScriptInfo
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public bool IsLoaded { get; internal set; }
        public string Error { get; internal set; }
        public DateTime? LoadedAt { get; internal set; }
    }
}
