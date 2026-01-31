using AliceInCradleHack.Utils;
using HarmonyLib;
using System;

namespace AliceInCradleHack.Events
{
    public static class DamageEvents
    {
        public static event EventHandler<PreDamageEventArgs> EventPreGetDamage;

        public static event EventHandler<PostDamageEventArgs> EventPostGetDamage;

        public static event EventHandler<PreDamageEventArgs> EventPrePlayerGetDamageHandler;

        public static event EventHandler<PostDamageEventArgs> EventPostPlayerGetDamageHandler;

        public static event EventHandler<PreDamageEventArgs> EventPreNotPlayerGetDamageHandler;

        public static event EventHandler<PostDamageEventArgs> EventPostNotPlayerGetDamageHandler;

        public static event EventHandler<PreDamageEventArgs> EventPreEnemyGetDamageHandler;

        public static event EventHandler<PostDamageEventArgs> EventPostEnemyGetDamageHandler;

        private readonly static Type typeNoel = Player.typeNoel;

        private readonly static Type typeEnemy = Type.GetType("nel.NelEnemy, Assembly-CSharp");

        private readonly static Harmony harmony = new Harmony("aliceincradlehack.events.damageevents");

        public static void Initialize()
        {
            harmony.Patch(
                AccessTools.Method("m2d.M2Attackable:applyHpDamage"),
                prefix: new HarmonyMethod(typeof(DamageEvents), nameof(PreFix)),
                postfix: new HarmonyMethod(typeof(DamageEvents), nameof(PostFix))
            );
        }

        public static void PreFix(object __instance, object[] __args)
        {
            var eventArgs = new PreDamageEventArgs(__instance, __args);

            try
            {
                EventPreGetDamage?.Invoke(__instance, eventArgs);
                if (__instance.GetType() == typeNoel)
                {
                    EventPrePlayerGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                else if (__instance.GetType().IsSubclassOf(typeEnemy))
                {
                    EventPreEnemyGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                if (__instance.GetType() != typeNoel)
                {
                    EventPreNotPlayerGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                
                __args[0] = eventArgs.val;
                __args[1] = eventArgs.force;
                __args[2] = eventArgs.AttackInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AliceInCradleHack][DamageEvents] PreFix exception: {ex}");
            }
        }

        public static void PostFix(object __instance, ref int __result, object[] __args)
        {
            var eventArgs = new PostDamageEventArgs(__instance, __result, __args);
            try
            {
                EventPostGetDamage?.Invoke(__instance, eventArgs);
                if (__instance.GetType() == typeNoel)
                {
                    EventPostPlayerGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                else if (__instance.GetType().IsSubclassOf(typeEnemy))
                {
                    EventPostEnemyGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                if (__instance.GetType() != typeNoel)
                {
                    EventPostNotPlayerGetDamageHandler?.Invoke(__instance, eventArgs);
                }
                __result = eventArgs.result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AliceInCradleHack][DamageEvents] PostFix exception: {ex}");
            }
        }

        public class PreDamageEventArgs : EventArgs
        {
            public object instance;

            public int val;

            public bool force;

            // m2d.AttackInfo
            public object AttackInfo;

            public PreDamageEventArgs(object instance, object[] args)
            {
                this.instance = instance;
                val = (int)args[0];
                force = (bool)args[1];
                AttackInfo = args[2];
            }
        }

        public class PostDamageEventArgs : EventArgs
        {
            public object instance;

            public int val;

            public bool force;

            // m2d.AttackInfo
            public object AttackInfo;

            public int result;

            public PostDamageEventArgs(object instance,int result, object[] args)
            {
                val = (int)args[0];
                force = (bool)args[1];
                AttackInfo = args[2];
                this.result = result;
                this.instance = instance;
            }
        }
    }
}