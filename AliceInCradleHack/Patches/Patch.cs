namespace AliceInCradleHack.Patches
{
    public abstract class Patch
    {
        // This method will be invoked when the hack is initializing.
        public abstract void Apply();

        // This method will be invoked when the hack is being removed or disabled.
        public abstract void Remove();
    }
}