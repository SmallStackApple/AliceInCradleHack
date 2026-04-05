using nel;
using System;

namespace AliceInCradleHack.Utils.Game.Objects
{
    public static class Player
    {
        public readonly static Type typeNoel = typeof(PRNoel);

        public static PRNoel Instance => NelM2DBase.PlayerNoel;

        public static int hp => M2Attackable.GetHp(SceneGame.PrNoelInstance);

        public static int maxhp => M2Attackable.GetMaxHp(SceneGame.PrNoelInstance);

        public static int mp => M2Attackable.GetMp(SceneGame.PrNoelInstance);

        public static int maxmp => M2Attackable.GetMaxMp(SceneGame.PrNoelInstance);
    }
}
