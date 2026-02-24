using System.Reflection;

namespace AliceInCradleHack.Utils
{
    public static class UseItemSelector
    {
        public static readonly FieldInfo fieldInfoACell = typeof(nel.UseItemSelector).GetField("ACell", BindingFlags.NonPublic | BindingFlags.Instance);

        public static nel.UseItemSelector Instance => NelItemManager.Instance?.USel;

        public static nel.UseItemSelector.ItCell[] ACell => Instance != null ? (nel.UseItemSelector.ItCell[])fieldInfoACell.GetValue(Instance) : null;
    }
}