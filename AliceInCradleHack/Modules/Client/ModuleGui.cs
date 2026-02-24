using System.Windows.Forms;

namespace AliceInCradleHack.Modules.Client
{
    public class ModuleGui : Module
    {
        public override string Name => "Gui";

        public override string Description => "The Heads-Up Display (HUD) module.";

        public override string Author => "SmallStackApple";

        public override string Version => "0.0.0";

        public override bool IsEnabled { get; set; } = true;

        public override string Category => "Client";

        public override SettingNode Settings { get; } = new SettingBuilder()
            .Add("ShowDynamicIsland","Show dynamic island on the top of the screen.", true)
            .Build();

        private readonly GuiForm guiForm = new GuiForm();

        public override void Initialize()
        {
            guiForm.Show();
        }

        public override void Enable()
        {
            guiForm.Show();

        }

        public override void Disable()
        {
            guiForm.Hide();
        }

        private class GuiForm : Form
        {
            
        }
    }
}