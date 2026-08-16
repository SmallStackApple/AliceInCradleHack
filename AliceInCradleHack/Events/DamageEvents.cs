using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using m2d;
using nel;
using System;

namespace AliceInCradleHack.events
{
    public static class DamageEvents
    {
        public static class Knockback
        {
            public static event EventHandler<PreKnockbackEventArgs> EventPreKnockback;

            /// <summary>
            /// Fires the pre-knockback event. Returns true when a handler cancelled the knockback.
            /// </summary>
            public static bool PreKnockback(object instance, ref float v0, ref AttackInfo attackInfo, ref m2d.M2Attackable another)
            {
                if (instance == null) return false;

                try
                {
                    var eventArgs = new PreKnockbackEventArgs(instance, v0, attackInfo, another);
                    InvokeHandlers(EventPreKnockback, instance, eventArgs);
                    v0 = eventArgs.V0;
                    attackInfo = eventArgs.AttackInfo;
                    another = eventArgs.Another;
                    return eventArgs.Cancel;
                }
                catch (Exception ex)
                {
                    Log.Error("DamageEvents PreKnockback exception", ex);
                    return false;
                }
            }

            public class PreKnockbackEventArgs : EventArgs
            {
                public object Instance { get; }
                public float V0 { get; set; }
                public AttackInfo AttackInfo { get; set; }
                public m2d.M2Attackable Another { get; set; }

                /// <summary>Set to true to skip the original knockback.</summary>
                public bool Cancel { get; set; }

                public PreKnockbackEventArgs(object instance, float v0, AttackInfo attackInfo, m2d.M2Attackable another)
                {
                    Instance = instance;
                    V0 = v0;
                    AttackInfo = attackInfo;
                    Another = another;
                }
            }
        }

        public static class HpDamage
        {
            public static event EventHandler<PreDamageEventArgs> EventPreGetDamage;
            public static event EventHandler<PostDamageEventArgs> EventPostGetDamage;
            public static event EventHandler<PreDamageEventArgs> EventPrePlayerGetDamageHandler;
            public static event EventHandler<PostDamageEventArgs> EventPostPlayerGetDamageHandler;
            public static event EventHandler<PreDamageEventArgs> EventPreNotPlayerGetDamageHandler;
            public static event EventHandler<PostDamageEventArgs> EventPostNotPlayerGetDamageHandler;
            public static event EventHandler<PreDamageEventArgs> EventPreEnemyGetDamageHandler;
            public static event EventHandler<PostDamageEventArgs> EventPostEnemyGetDamageHandler;

            private static readonly Type _typeNoel = Player.TypeNoel;
            private static readonly Type _typeEnemy = typeof(NelEnemy);

            private static void Dispatch<TArgs>(object instance, TArgs eventArgs,
                EventHandler<TArgs> allHandler,
                EventHandler<TArgs> playerHandler,
                EventHandler<TArgs> enemyHandler,
                EventHandler<TArgs> notPlayerHandler) where TArgs : EventArgs
            {
                var instanceType = instance.GetType();

                InvokeHandlers(allHandler, instance, eventArgs);
                if (_typeNoel.IsAssignableFrom(instanceType))
                {
                    InvokeHandlers(playerHandler, instance, eventArgs);
                }
                else if (_typeEnemy.IsAssignableFrom(instanceType))
                {
                    InvokeHandlers(enemyHandler, instance, eventArgs);
                }
                if (!_typeNoel.IsAssignableFrom(instanceType))
                {
                    InvokeHandlers(notPlayerHandler, instance, eventArgs);
                }
            }

            public static void PreDamage(object instance, object[] args)
            {
                if (instance == null) return;

                try
                {
                    var eventArgs = new PreDamageEventArgs(instance, args);
                    Dispatch(instance, eventArgs,
                        EventPreGetDamage,
                        EventPrePlayerGetDamageHandler,
                        EventPreEnemyGetDamageHandler,
                        EventPreNotPlayerGetDamageHandler);

                    args[0] = eventArgs.Val;
                    args[1] = eventArgs.Force;
                    args[2] = eventArgs.AttackInfo;
                }
                catch (Exception ex)
                {
                    Log.Error("DamageEvents PreFix exception", ex);
                }
            }

            public static void PostDamage(object instance, ref int result, object[] args)
            {
                if (instance == null) return;

                try
                {
                    var eventArgs = new PostDamageEventArgs(instance, result, args);
                    Dispatch(instance, eventArgs,
                        EventPostGetDamage,
                        EventPostPlayerGetDamageHandler,
                        EventPostEnemyGetDamageHandler,
                        EventPostNotPlayerGetDamageHandler);

                    result = eventArgs.Result;
                }
                catch (Exception ex)
                {
                    Log.Error("DamageEvents PostFix exception", ex);
                }
            }

            public class PreDamageEventArgs : EventArgs
            {
                public object Instance { get; }
                public int Val { get; set; }
                public bool Force { get; set; }
                public AttackInfo AttackInfo { get; set; }

                public PreDamageEventArgs(object instance, object[] args)
                {
                    Instance = instance;
                    Val = (int)args[0];
                    Force = (bool)args[1];
                    AttackInfo = (AttackInfo)args[2];
                }
            }

            public class PostDamageEventArgs : EventArgs
            {
                public object Instance { get; }
                public int Val { get; }
                public bool Force { get; }
                public AttackInfo AttackInfo { get; }
                public int Result { get; set; }

                public PostDamageEventArgs(object instance, int result, object[] args)
                {
                    Instance = instance;
                    Val = (int)args[0];
                    Force = (bool)args[1];
                    AttackInfo = (AttackInfo)args[2];
                    Result = result;
                }
            }
        }

        private static void InvokeHandlers<TArgs>(EventHandler<TArgs> handlers, object sender, TArgs eventArgs)
            where TArgs : EventArgs
        {
            if (handlers == null) return;

            foreach (EventHandler<TArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler.Invoke(sender, eventArgs);
                }
                catch (Exception ex)
                {
                    Log.Error("DamageEvents handler exception", ex);
                }
            }
        }
    }
}
