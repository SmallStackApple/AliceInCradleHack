using System;
using System.Reflection;

namespace AliceInCradleHack.Utils
{
    public static class SceneGame
    {
        public readonly static Type typeSceneGame = typeof(nel.SceneGame);

        public readonly static FieldInfo fieldInfoPlayer = typeSceneGame.GetField("PrNoel", BindingFlags.NonPublic | BindingFlags.Instance);

        public readonly static FieldInfo fieldInfoM2D = typeSceneGame.GetField("M2D", BindingFlags.NonPublic | BindingFlags.Instance);

        public static nel.SceneGame Instance { get; set; } = null;

        public static nel.PRNoel PrNoelInstance { get; set; } = null;

        public static nel.NelM2DBase M2DInstance { get; set; } = null;
    }
}