using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using nel;

namespace AliceInCradleHack.Utils
{
    public static class SceneGame
    {
        public readonly static Type typeSceneGame = typeof(nel.SceneGame);

        public readonly static FieldInfo fieldInfoPlayer = typeSceneGame.GetField("PrNoel", BindingFlags.NonPublic | BindingFlags.Instance);

        public static PRNoel PlayerInstance { get; set; } = null;
    }
}