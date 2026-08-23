namespace AliceInCradleHack.utils.game
{
    public static class NelM2DBase
    {
        public static nel.NelM2DBase Instance => m2d.M2DBase.Instance as nel.NelM2DBase;

        public static nel.PRNoel PlayerNoel => Instance?.PlayerNoel;
    }
}
