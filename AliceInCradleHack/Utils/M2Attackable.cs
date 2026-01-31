using System;
using System.Reflection;
namespace AliceInCradleHack.Utils
{
    public static class M2Attackable
    {
        public readonly static Type typeM2Attackable = Type.GetType("m2d.M2Attackable, unsafeAssem");

        public readonly static FieldInfo fieldInfoHp = typeM2Attackable.GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMaxHp = typeM2Attackable.GetField("maxhp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMp = typeM2Attackable.GetField("mp", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoMaxMp = typeM2Attackable.GetField("maxmp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetHp(object instance) => (int)fieldInfoHp.GetValue(instance);

        public static int GetMaxHp(object instance) => (int)fieldInfoMaxHp.GetValue(instance);

        public static int GetMp(object instance) => (int)fieldInfoMp.GetValue(instance);

        public static int GetMaxMp(object instance) => (int)fieldInfoMaxMp.GetValue(instance);
    }
}
