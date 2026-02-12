using System;
using System.Reflection;
using m2d;

namespace AliceInCradleHack.Utils
{
    public static class M2Attackable
    {
        public readonly static Type typeM2Attackable = typeof(m2d.M2Attackable);

        public readonly static FieldInfo fieldInfoHp = typeM2Attackable.GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMaxHp = typeM2Attackable.GetField("maxhp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMp = typeM2Attackable.GetField("mp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMaxMp = typeM2Attackable.GetField("maxmp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetHp(m2d.M2Attackable instance) => instance == null ? -1 : (fieldInfoHp.GetValue(instance) as int? ?? -1);

        public static int GetMaxHp(m2d.M2Attackable instance) => instance == null ? -1 : (fieldInfoMaxHp.GetValue(instance) as int? ?? -1);

        public static int GetMp(m2d.M2Attackable instance) => instance == null ? -1 : (fieldInfoMp.GetValue(instance) as int? ?? -1);

        public static int GetMaxMp(m2d.M2Attackable instance) => instance == null ? -1 : (fieldInfoMaxMp.GetValue(instance) as int? ?? -1);
    }
}