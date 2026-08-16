namespace AliceInCradleHack.module.modules.client.island
{
    public interface IHudElement
    {
        public enum Alignment
        {
            Top,
            Bottom,
            Left,
            Right,
            Center
        }

        public readonly struct Size
        {
            public readonly float Width;
            public readonly float Height;

            public Size(float width, float height)
            {
                Width = width;
                Height = height;
            }
        }

        bool IsVisible { get; }

        bool HasBackground { get; }

        Alignment HudAnchor { get; }

        Size HudSize { get; }

        void Render(float x, float y, float width, float height, float alpha);
    }
}
