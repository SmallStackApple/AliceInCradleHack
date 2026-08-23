using System.Reflection;

namespace AliceInCradleHack.utils.game
{
    public static class UseItemSelector
    {
        public static readonly FieldInfo FieldInfoACell = typeof(nel.UseItemSelector).GetField("ACell", BindingFlags.NonPublic | BindingFlags.Instance);

        public static nel.UseItemSelector Instance => NelItemManager.Instance?.USel;

        public static nel.UseItemSelector.ItCell[] ACell => GetACell(Instance);

        private static nel.UseItemSelector.ItCell[] GetACell(nel.UseItemSelector instance)
        {
            if (instance == null || FieldInfoACell == null) return null;

            try
            {
                return FieldInfoACell.GetValue(instance) as nel.UseItemSelector.ItCell[];
            }
            catch
            {
                return null;
            }
        }
    }
}
