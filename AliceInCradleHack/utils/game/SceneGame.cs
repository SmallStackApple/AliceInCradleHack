using System;
using System.Reflection;

namespace AliceInCradleHack.utils.game
{
    /// <summary>
    /// Static accessors for the running game scene, kept up to date by PatchNelSceneGame.
    /// </summary>
    public static class SceneGame
    {
        public static readonly Type TypeSceneGame = typeof(nel.SceneGame);

        public static readonly FieldInfo FieldInfoPlayer = TypeSceneGame.GetField("PrNoel", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo FieldInfoM2D = TypeSceneGame.GetField("M2D", BindingFlags.NonPublic | BindingFlags.Instance);

        public static nel.SceneGame Instance { get; set; }

        public static nel.PRNoel PrNoelInstance { get; set; }

        public static nel.NelM2DBase M2DInstance { get; set; }
    }
}
