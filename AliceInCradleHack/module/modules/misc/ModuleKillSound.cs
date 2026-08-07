using AliceInCradleHack.module.settings;
using AliceInCradleHack.utils.game;
using NAudio.Wave;
using System;
using System.IO;
using static AliceInCradleHack.events.DamageEvents;

namespace AliceInCradleHack.module.modules.misc
{
    public class ModuleKillSound : Module
    {
        public override string Name => "KillSound";
        public override string Description => "Plays a sound when you kill an enemy.";
        public override string Author => "SmallStackApple";
        public override string Version => "1.0.0";
        public override string Category => "Misc";

        // Persistent settings instance so runtime changes (via commands) apply to this module.
        public override SettingNode Settings { get; } = new SettingBuilder()
            .Add("Volume", "Volume of the kill sound (0-100).", 100)
            .Add("SoundFilePath", "Path to the sound file to play on kill.", "kill_sound.wav")
            .Build();

        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileReader;

        public override void Enable()
        {
            HpDamage.EventPostEnemyGetDamageHandler += PlayKillSound;
        }

        public override void Disable()
        {
            HpDamage.EventPostEnemyGetDamageHandler -= PlayKillSound;
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
                Console.WriteLine("Error disposing audio resources: " + ex.Message);
            }
        }

        private void PlayKillSound(object sender, HpDamage.PostDamageEventArgs eventArgs)
        {
            if (M2Attackable.GetHp((m2d.M2Attackable)sender) != 0 || eventArgs.AttackInfo.AttackFrom.GetType() != Player.TypeNoel)
                return;

            string soundFilePath = (string)Settings.GetValueByPath("SoundFilePath");
            if (string.IsNullOrWhiteSpace(soundFilePath))
            {
                Console.WriteLine("Kill sound file path is empty.");
                return;
            }

            if (!File.Exists(soundFilePath))
            {
                Console.WriteLine($"Kill sound file not found: {soundFilePath}");
                return;
            }

            try
            {
                DisposeAudio();

                _audioFileReader = new AudioFileReader(soundFilePath);

                // Volume setting is stored as 0-100, convert to 0.0-1.0.
                var volObj = Settings.GetValueByPath("Volume");
                float vol = 1.0f;
                if (volObj is int vi)
                    vol = Math.Max(0, Math.Min(100, vi)) / 100f;
                else if (volObj is float vf)
                    vol = Math.Max(0f, Math.Min(1f, vf));
                _audioFileReader.Volume = vol;

                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioFileReader);
                _outputDevice.PlaybackStopped += (s, e) => DisposeAudio();
                _outputDevice.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing kill sound (NAudio): " + ex.Message);
                DisposeAudio();
            }
        }
    }
}
