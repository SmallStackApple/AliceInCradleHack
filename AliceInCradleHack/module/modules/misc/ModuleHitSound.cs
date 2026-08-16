using AliceInCradleHack.config;
using AliceInCradleHack.utils.game;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleHitSound : ModuleSoundBase
    {
        public ModuleHitSound() : base("HitSound", "Plays a sound when you hit an enemy.", "Misc")
        {
        }

        public readonly RangedValue<int> Volume = new(100, 0, 100, "%", "Volume of the hit sound (0-100).");

        public readonly Value<string> SoundFilePath = new("hit_sound.wav", "Path to the sound file to play on hit.");

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
            PlaySound(SoundFilePath);
        }
    }
}
