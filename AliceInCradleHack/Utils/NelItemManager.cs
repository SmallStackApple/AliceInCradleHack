namespace AliceInCradleHack.Utils
{
    public static class NelItemManager
    {
        public static nel.NelItemManager Instance => NelM2DBase.Instance?.IMNG;
    }
}