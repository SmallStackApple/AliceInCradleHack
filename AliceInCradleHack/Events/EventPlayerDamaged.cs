using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AliceInCradleHack.Events
{
    public static class EventPlayerDamaged
    {
        // EventArg: int - amount of damage taken
        public static event EventHandler Handler;

        private static readonly Type targetClass = Type.GetType("nel.PRNoel, Assembly-CSharp");

        private static void PostFix(object __instance, ref int __result)
        {
            if (__result > 0 && __instance.GetType() == targetClass)
                Handler?.Invoke(__instance, new Event.IntEventArg(__result));
        }

        private static readonly Harmony harmony = new Harmony("aliceincradlehack.events.eventplayerdamaged");

        // Called by EventManager at startup
        public static void Initialize()
        {
            harmony.Patch(
                AccessTools.Method("m2d.M2Attackable:applyHpDamage"),
                postfix: new HarmonyMethod(typeof(EventPlayerDamaged), nameof(PostFix))
            );
        }
    }
}
