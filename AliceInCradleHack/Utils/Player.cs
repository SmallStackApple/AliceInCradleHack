using nel;
using System;

namespace AliceInCradleHack.Utils
{
    public static class Player
    {
        public readonly static Type typeNoel = typeof(PRNoel);

        public static PRNoel Instance => SceneGame.PlayerInstance;

        public static int hp
        {
            get
            {
                return M2Attackable.GetHp(SceneGame.PlayerInstance);
            }
        }

        public static int maxhp
        {
            get
            {
                return M2Attackable.GetMaxHp(SceneGame.PlayerInstance);
            }
        }

        public static int mp
        {
            get
            {
                return M2Attackable.GetMp(SceneGame.PlayerInstance);
            }
        }

        public static int maxmp
        {
            get
            {
                return M2Attackable.GetMaxMp(SceneGame.PlayerInstance);
            }
        }
    }
}
