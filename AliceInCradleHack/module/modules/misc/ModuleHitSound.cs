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

        private readonly object _audioLock = new();
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileReader;

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

        /// <summary>Plays the given sound file, stopping any sound that is still playing.</summary>
        private void PlaySound(string soundFilePath)
        {
            if (string.IsNullOrWhiteSpace(soundFilePath))
            {
                Log.Warn($"{Name} sound file not found: path is empty.");
                return;
            }

            if (!File.Exists(soundFilePath))
            {
                Log.Warn($"{Name} sound file not found: {soundFilePath}");
                return;
            }

            try
            {
                DisposeAudio();

                _audioFileReader = new AudioFileReader(soundFilePath)
                {
                    Volume = Volume.Get() / 100f
                };

                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioFileReader);
                _outputDevice.PlaybackStopped += (s, e) => DisposeAudio();
                _outputDevice.Play();
            }
            catch (Exception ex)
            {
                Log.Error($"Error playing {Name} sound (NAudio)", ex);
                DisposeAudio();
            }
        }

        private void DisposeAudio()
        {
            WaveOutEvent outputDevice;
            AudioFileReader audioFileReader;
            lock (_audioLock)
            {
                // Clear the shared state before Stop raises PlaybackStopped synchronously.
                outputDevice = _outputDevice;
                audioFileReader = _audioFileReader;
                _outputDevice = null;
                _audioFileReader = null;
            }

            try
            {
                outputDevice?.Stop();
                outputDevice?.Dispose();
                audioFileReader?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Error disposing audio resources", ex);
            }
        }
    }
}
