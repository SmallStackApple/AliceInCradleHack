using nel;
using System;

namespace AliceInCradleHack.utils.game
{
    public static class Player
    {
        public static readonly Type TypeNoel = typeof(PRNoel);

        public static PRNoel Instance => NelM2DBase.PlayerNoel;

        public static int Hp => M2Attackable.GetHp(SceneGame.PrNoelInstance);

        public static int MaxHp => M2Attackable.GetMaxHp(SceneGame.PrNoelInstance);

        public static int Mp => M2Attackable.GetMp(SceneGame.PrNoelInstance);

        public static int MaxMp => M2Attackable.GetMaxMp(SceneGame.PrNoelInstance);
    }
}
