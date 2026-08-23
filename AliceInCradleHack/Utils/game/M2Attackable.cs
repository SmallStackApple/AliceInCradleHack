using System;
using System.Reflection;

namespace AliceInCradleHack.utils.game
{
    /// <summary>
    /// Reflection-based accessor for m2d.M2Attackable stat fields.
    /// Returns -1 when the instance is null or the game field cannot be found.
    /// </summary>
    public static class M2Attackable
    {
        public static readonly Type TypeM2Attackable = typeof(m2d.M2Attackable);

        public static readonly FieldInfo FieldInfoHp = TypeM2Attackable.GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMaxHp = TypeM2Attackable.GetField("maxhp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMp = TypeM2Attackable.GetField("mp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoMaxMp = TypeM2Attackable.GetField("maxmp", BindingFlags.NonPublic | BindingFlags.Instance);

        public static int GetHp(m2d.M2Attackable instance) => GetStat(instance, FieldInfoHp);

        public static int GetMaxHp(m2d.M2Attackable instance) => GetStat(instance, FieldInfoMaxHp);

        public static int GetMp(m2d.M2Attackable instance) => GetStat(instance, FieldInfoMp);

        public static int GetMaxMp(m2d.M2Attackable instance) => GetStat(instance, FieldInfoMaxMp);

        private static int GetStat(m2d.M2Attackable instance, FieldInfo field)
        {
            if (instance == null || field == null) return -1;
            try
            {
                return field.GetValue(instance) as int? ?? -1;
            }
            catch
            {
                return -1;
            }
        }
    }
}
