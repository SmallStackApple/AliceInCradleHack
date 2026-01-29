using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;

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

        // Use a persistent SettingNode instance so runtime changes (via commands)
        // are applied to the same settings object instead of recreating it on each access.
        public override SettingNode Settings { get; } = new SettingBuilder()
            .Add("Volume", "Volume of the kill sound (0-100).", 100)
            .Add("SoundFilePath", "Path to the sound file to play on kill.", "kill_sound.wav")
            .Build();

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;
        private EventHandler eventHandler;
        public override void Disable()
        {
            Events.EventNotPlayerDamaged.Handler -= eventHandler;
            IsEnabled = false;
            try
            {
                outputDevice?.Stop();
                outputDevice?.Dispose();
                outputDevice = null;
                audioFileReader?.Dispose();
                audioFileReader = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error disposing audio resources: " + ex.Message);
            }
        }
        public override void Enable()
        {
            Events.EventNotPlayerDamaged.Handler += eventHandler;
            IsEnabled = true;
        }
        public override void Initialize()
        {
            eventHandler = new EventHandler((sender, args) =>
            {
                var eventArgs = args as Event.ObjectListEventArg;
                if ((int)eventArgs.Objects[1] == 0)
                {
                    PlayKillSound();
                }
            });
        }

        private void PlayKillSound()
        {
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
                // Dispose any previous resources
                outputDevice?.Stop();
                outputDevice?.Dispose();
                outputDevice = null;
                audioFileReader?.Dispose();
                audioFileReader = null;

                audioFileReader = new AudioFileReader(soundFilePath);

                // Volume setting stored as 0-100, convert to 0.0-1.0
                var volObj = Settings.GetValueByPath("Volume");
                float vol = 1.0f;
                if (volObj is int vi)
                    vol = Math.Max(0, Math.Min(100, vi)) / 100f;
                else if (volObj is float vf)
                    vol = Math.Max(0f, Math.Min(1f, vf));
                audioFileReader.Volume = vol;

                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFileReader);
                outputDevice.PlaybackStopped += (s, e) =>
                {
                    // Dispose reader when playback completes
                    try
                    {
                        audioFileReader?.Dispose();
                        audioFileReader = null;
                        outputDevice?.Dispose();
                        outputDevice = null;
                    }
                    catch { }
                };
                outputDevice.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing kill sound (NAudio): " + ex.Message);
                try
                {
                    audioFileReader?.Dispose();
                    audioFileReader = null;
                    outputDevice?.Dispose();
                    outputDevice = null;
                }
                catch { }
            }
        }
    }
}
