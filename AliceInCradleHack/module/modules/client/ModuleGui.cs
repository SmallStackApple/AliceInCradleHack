using AliceInCradleHack.config;
using System.Windows.Forms;

namespace AliceInCradleHack.module.modules.client
{
    public class ModuleGui : Module
    {
        public ModuleGui() : base("Gui", "The Heads-Up Display (HUD) module.", "Client")
        {
        }

        public override bool IsEnabled { get; set; } = true;

        public readonly Value<bool> ShowDynamicIsland = new(true, "Show dynamic island on the top of the screen.");

        private readonly GuiForm _guiForm = new GuiForm();

        public override void Initialize()
        {
            _guiForm.Show();
        }

        public override void Enable()
        {
            _guiForm.Show();
        }

        public override void Disable()
        {
            _guiForm.Hide();
        }

        private class GuiForm : Form
        {
        }
    }
}
