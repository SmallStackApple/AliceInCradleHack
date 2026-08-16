using nel;
using System;

namespace AliceInCradleHack.utils.game
{
    /// <summary>
    /// Static accessors for the player character (Noel). Returns -1 for stats while
    /// no player exists or the underlying game fields cannot be read.
    /// </summary>
    public static class Player
    {
        public static readonly Type TypeNoel = typeof(PRNoel);

        public static PRNoel Instance => NelM2DBase.PlayerNoel;

        public static int Hp => M2Attackable.GetHp(Instance);

        public static int MaxHp => M2Attackable.GetMaxHp(Instance);

        public static int Mp => M2Attackable.GetMp(Instance);

        public static int MaxMp => M2Attackable.GetMaxMp(Instance);
    }
}
