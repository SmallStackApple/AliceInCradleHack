using AliceInCradleHack.config;
using AliceInCradleHack.utils.game;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleKillSound : ModuleSoundBase
    {
        public ModuleKillSound() : base("KillSound", "Plays a sound when you kill an enemy.", "Misc")
        {
        }

        public readonly RangedValue<int> Volume = new(100, 0, 100, "%", "Volume of the kill sound (0-100).");

        public readonly Value<string> SoundFilePath = new("kill_sound.wav", "Path to the sound file to play on kill.");

        protected override float VolumeFactor => Volume.Get() / 100f;

        public override void Enable()
        {
            HpDamage.EventPostEnemyGetDamageHandler += OnEnemyPostDamage;
        }

        public override void Disable()
        {
            HpDamage.EventPostEnemyGetDamageHandler -= OnEnemyPostDamage;
            DisposeAudio();
        }

        private void OnEnemyPostDamage(object sender, HpDamage.PostDamageEventArgs eventArgs)
        {
            if (!ReferenceEquals(eventArgs.AttackInfo?.AttackFrom, NelM2DBase.PlayerNoel)) return;
            if (M2Attackable.GetHp(sender as m2d.M2Attackable) != 0) return;
            PlaySound(SoundFilePath);
        }
    }
}
