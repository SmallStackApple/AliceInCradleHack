using AliceInCradleHack.utils.game;
using m2d;
using nel;
using System;

namespace AliceInCradleHack.events
{
    public static class DamageEvents
    {
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

            private static readonly Type TypeNoel = Player.TypeNoel;
            private static readonly Type TypeEnemy = typeof(NelEnemy);

            private static void Dispatch<TArgs>(object instance, TArgs eventArgs,
                EventHandler<TArgs> allHandler,
                EventHandler<TArgs> playerHandler,
                EventHandler<TArgs> enemyHandler,
                EventHandler<TArgs> notPlayerHandler) where TArgs : EventArgs
            {
                var instanceType = instance.GetType();

                allHandler?.Invoke(instance, eventArgs);
                if (instanceType == TypeNoel)
                {
                    playerHandler?.Invoke(instance, eventArgs);
                }
                else if (instanceType.IsSubclassOf(TypeEnemy))
                {
                    enemyHandler?.Invoke(instance, eventArgs);
                }
                if (instanceType != TypeNoel)
                {
                    notPlayerHandler?.Invoke(instance, eventArgs);
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
                    Console.WriteLine($"[AliceInCradleHack][DamageEvents] PreFix exception: {ex}");
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
                    Console.WriteLine($"[AliceInCradleHack][DamageEvents] PostFix exception: {ex}");
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
    }
}
