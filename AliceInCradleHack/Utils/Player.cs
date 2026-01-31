using System;
using System.Reflection;

namespace AliceInCradleHack.Utils
{
    public static class Player
    {
        public readonly static Type typeNoel = Type.GetType("nel.PRNoel, Assembly-CSharp");

        public static int GetHp(object instance) => M2Attackable.GetHp(instance);

        public static int GetMaxHp(object instance) => M2Attackable.GetMaxHp(instance);

        public static int GetMp(object instance) => M2Attackable.GetMp(instance);

        public static int GetMaxMp(object instance) => M2Attackable.GetMaxMp(instance);
    }
}
