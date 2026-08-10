using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using AliceInCradleHack.utils.game;
using NAudio.Wave;
using System;
using System.IO;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleHitSound : Module
    {
        public ModuleHitSound() : base("HitSound", "Plays a sound when you hit an enemy.", "Misc")
        {
        }

        public readonly RangedValue<int> Volume = new(100, 0, 100, "%", "Volume of the hit sound (0-100).");

        public readonly Value<string> SoundFilePath = new("hit_sound.wav", "Path to the sound file to play on hit.");

        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileReader;

        public override void Enable()
        {
            HpDamage.EventPostEnemyGetDamageHandler += PlayHitSound;
        }

        public override void Disable()
        {
            HpDamage.EventPostEnemyGetDamageHandler -= PlayHitSound;
            DisposeAudio();
        }

        public override void Initialize()
        {
        }

        private void DisposeAudio()
        {
            try
            {
                _outputDevice?.Stop();
                _outputDevice?.Dispose();
                _outputDevice = null;
                _audioFileReader?.Dispose();
                _audioFileReader = null;
            }
            catch (Exception ex)
            {
                Log.Error("Error disposing audio resources", ex);
            }
        }

        private void PlayHitSound(object sender, HpDamage.PostDamageEventArgs eventArgs)
        {
            if (!ReferenceEquals(eventArgs.AttackInfo.AttackFrom, NelM2DBase.PlayerNoel)) return;

            string soundFilePath = SoundFilePath;
            if (string.IsNullOrWhiteSpace(soundFilePath))
            {
                Log.Warn("Hit sound file not found: path is empty.");
                return;
            }

            if (!File.Exists(soundFilePath))
            {
                Log.Warn($"Hit sound file not found: {soundFilePath}");
                return;
            }

            try
            {
                DisposeAudio();

                _audioFileReader = new AudioFileReader(soundFilePath);
                _audioFileReader.Volume = Volume.Get() / 100f;

                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioFileReader);
                _outputDevice.PlaybackStopped += (s, e) => DisposeAudio();
                _outputDevice.Play();
            }
            catch (Exception ex)
            {
                Log.Error("Error playing hit sound (NAudio)", ex);
                DisposeAudio();
            }
        }
    }
}
