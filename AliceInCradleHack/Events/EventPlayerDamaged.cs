using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AliceInCradleHack.Events
{
    public static class EventPlayerDamaged
    {
        // ObjectListEventArg: [int - amount of damage taken,int - hp remain]
        public static event EventHandler Handler;

        private static readonly Type targetClass = Type.GetType("nel.PRNoel, Assembly-CSharp");

        private static void PostFix(object __instance, ref int __result)
        {
            if (__result > 0 && __instance.GetType() == targetClass)
                Handler?.Invoke(
                    __instance, 
                    new Event.ObjectListEventArg(
                        new List<object>
                        {
                            __result,
                            __instance.GetType().GetField("hp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) 
                        }
                    )
                );
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
