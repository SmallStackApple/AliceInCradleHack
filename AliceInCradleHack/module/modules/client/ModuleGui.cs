namespace AliceInCradleHack.module.modules.client
{
    public class ModuleGui : Module
    {
        public ModuleGui() : base("Gui", "The Heads-Up Display (HUD) module.", "Client")
        {
        }

        public override bool IsEnabled { get; set; } = true;
    }
}
