using System.Reflection;

namespace AliceInCradleHack.utils.game
{
    public static class UseItemSelector
    {
        public static readonly FieldInfo FieldInfoACell = typeof(nel.UseItemSelector).GetField("ACell", BindingFlags.NonPublic | BindingFlags.Instance);

        public static nel.UseItemSelector Instance => NelItemManager.Instance?.USel;

        public static nel.UseItemSelector.ItCell[] ACell => Instance != null ? (nel.UseItemSelector.ItCell[])FieldInfoACell.GetValue(Instance) : null;
    }
}
