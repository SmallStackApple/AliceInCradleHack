using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceInCradleHack.Modules
{
    public class ModuleKillSound : Module
    {
        public override string Name => "KillSound";
        public override string Description => "Plays a sound when you kill an enemy.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override bool IsEnabled { get; set; } = false;
        public override string Category => "Misc";
        public override SettingNode Settings =>
            new SettingBuilder()
            .Add("SoundFilePath", "Path to the sound file to play on kill.", "kill_sound.wav")
            .Build();
        public override void Disable()
        {
            Events.EventNotPlayerDamaged.Handler -= (sender, args) =>
            {
                var eventArgs = (Event.ObjectListEventArg)args;
                int hp_remain = (int)eventArgs.Objects[1];
                PlayKillSound(hp_remain);
            };
            IsEnabled = false;
        }
        public override void Enable()
        {
            Events.EventNotPlayerDamaged.Handler += (sender, args) =>
            {
                var eventArgs = (Event.ObjectListEventArg)args;
                int hp_remain = (int)eventArgs.Objects[1];
                PlayKillSound(hp_remain);
            };
            IsEnabled = true;
        }
        public override void Initialize()
        {
        }

        private void PlayKillSound(int hp_remain)
        {
            if(hp_remain == 0)
            {
                string soundFilePath = (string)Settings.GetValueByPath("SoundFilePath");
                
            }
        }
    }
}
