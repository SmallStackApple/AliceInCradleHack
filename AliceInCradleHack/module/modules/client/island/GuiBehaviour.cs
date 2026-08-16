using AliceInCradleHack.utils.client;

namespace AliceInCradleHack.module.modules.client.island
{
    public class GuiBehaviour : OnGuiRenderer<GuiBehaviour>
    {
        public static void EnsureCreated()
        {
            EnsureCreated("AliceInCradleHack.Gui");
        }

        protected override void Render()
        {
            DynamicIsland.Instance.Render();
        }
    }
}
