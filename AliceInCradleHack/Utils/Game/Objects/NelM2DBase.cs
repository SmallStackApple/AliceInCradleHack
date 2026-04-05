namespace AliceInCradleHack.Utils.Game.Objects
{
    public static class NelM2DBase
    {
        public static nel.NelM2DBase Instance => (nel.NelM2DBase)nel.NelM2DBase.Instance;

        public static nel.PRNoel PlayerNoel => Instance?.PlayerNoel;
    }
}