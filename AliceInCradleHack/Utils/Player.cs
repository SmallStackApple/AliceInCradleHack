using nel;
using System;

namespace AliceInCradleHack.Utils
{
    public static class Player
    {
        public readonly static Type typeNoel = typeof(PRNoel);

        public static PRNoel Instance => SceneGame.PrNoelInstance;

        public static int hp
        {
            get
            {
                return M2Attackable.GetHp(SceneGame.PrNoelInstance);
            }
        }

        public static int maxhp
        {
            get
            {
                return M2Attackable.GetMaxHp(SceneGame.PrNoelInstance);
            }
        }

        public static int mp
        {
            get
            {
                return M2Attackable.GetMp(SceneGame.PrNoelInstance);
            }
        }

        public static int maxmp
        {
            get
            {
                return M2Attackable.GetMaxMp(SceneGame.PrNoelInstance);
            }
        }
    }
}
