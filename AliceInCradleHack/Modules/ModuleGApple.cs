using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Modules
{
    public class ModuleGApple : Module
    {
        public override string Name => "GApple";

        public override string Description => "Auto use buff item";

        public override string Author => "SmallStackApple";

        public override string Version => "1.0.0";

        public override bool IsEnabled { get; set; } = false;

        public override string Category => "Combat";

        public override SettingNode Settings =>
            new SettingBuilder()
            .Add("MinHP","Minimum HP percentage to activate GApple.", 50)
            .Add("Delay","Delay between GApple uses in seconds.", 2d)
            .Build();

        public override void Disable()
        {
        }

        public override void Enable()
        {
        }

        public override void Initialize()
        {
        }
    }
}
