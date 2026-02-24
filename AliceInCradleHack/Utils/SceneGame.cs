using System;
using System.Reflection;
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