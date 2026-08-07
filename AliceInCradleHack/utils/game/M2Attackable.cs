using System;
using System.Reflection;

namespace AliceInCradleHack.utils.game
{
    /// <summary>
    /// Reflection-based accessor for m2d.M2Attackable stat fields.
    /// </summary>
    public static class M2Attackable
    {
        public static readonly Type TypeM2Attackable = typeof(m2d.M2Attackable);

        public static readonly FieldInfo FieldInfoHp = TypeM2Attackable.GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMaxHp = TypeM2Attackable.GetField("maxhp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMp = TypeM2Attackable.GetField("mp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMaxMp = TypeM2Attackable.GetField("maxmp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetHp(m2d.M2Attackable instance) => instance == null ? -1 : (FieldInfoHp.GetValue(instance) as int? ?? -1);

        public static int GetMaxHp(m2d.M2Attackable instance) => instance == null ? -1 : (FieldInfoMaxHp.GetValue(instance) as int? ?? -1);

        public static int GetMp(m2d.M2Attackable instance) => instance == null ? -1 : (FieldInfoMp.GetValue(instance) as int? ?? -1);

        public static int GetMaxMp(m2d.M2Attackable instance) => instance == null ? -1 : (FieldInfoMaxMp.GetValue(instance) as int? ?? -1);
    }
}
