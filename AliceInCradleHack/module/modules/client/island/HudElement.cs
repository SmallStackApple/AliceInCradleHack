namespace AliceInCradleHack.module.modules.client.island
{
    public abstract class HudElement : IHudElement
    {
        public virtual bool IsVisible => true;

        public virtual bool HasBackground => false;

        public virtual IHudElement.Alignment HudAnchor => IHudElement.Alignment.Top;

        public virtual IHudElement.Size HudSize => new(240f, 25f);

        public virtual void Render(float x, float y, float width, float height, float alpha)
        {
        }
    }
}
