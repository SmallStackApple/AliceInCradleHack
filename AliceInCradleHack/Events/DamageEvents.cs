

namespace AliceInCradleHack.Events
{
    public static class DamageEvents
    {
        public static event EventHandler<DamageEventArgs> EventPreDamage;

        public static event EventHandler<DamageEventArgs> EventPostDamage;

        public static event EventHandler<DamageEventArgs> EventPrePlayerDamageHandler;

        public static event EventHandler<DamageEventArgs> EventPostPlayerDamageHandler;

        public static event EventHandler<DamageEventArgs> EventPreNotPlayerDamageHandler;

        public static event EventHandler<DamageEventArgs> EventPostNotPlayerGetDamageHandler;

        public static event EventHandler<DamageEventArgs> EventPreEnemyGetDamageHandler;

        public static event EventHandler<DamageEventArgs> EventPostEnemyGetDamageHandler;





        public class DamageEventArgs : EventArgs
        {
            public int val;

            public bool force;

            // m2d.AttackInfo
            public object AttackInfo;

            public DamageEventArgs(object[] args)
            {
                val = (int)args[0];
                force = (bool)args[1];
                AttackInfo = args[2];
            }
        }
    }
}