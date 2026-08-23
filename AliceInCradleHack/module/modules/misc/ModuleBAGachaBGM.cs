using AliceInCradleHack.config;
using AliceInCradleHack.utils.client;
using HarmonyLib;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AliceInCradleHack.module.modules.misc
{
    public enum BAGachaBgmChoice
    {
        old,
        @new
    }

    public sealed class ModuleBAGachaBGM : Module
    {
        private const string ResourcePrefix = ".resources.audio.";
        private static readonly string AudioFolder = Path.Combine(Path.GetTempPath(), "AliceInCradleHack", "BAGachaBGM");
        private static ModuleBAGachaBGM _instance;
        private readonly Harmony _harmony = new("aliceincradlehack.module.bagachabgm");

        public ModuleBAGachaBGM() : base("BAGachaBGM", "Plays selected BGM on the treasure chest reel page.", "Misc")
        {
            _instance = this;
        }

        public readonly EnumChoiceValue<BAGachaBgmChoice> Music = new(
            BAGachaBgmChoice.old,
            "Music played while the treasure chest reel page is open.");

        public readonly RangedValue<int> Volume = new(100, 0, 100, "%", "BGM volume (0-100).");

        internal static bool IsEnabledAndReady => _instance?.IsEnabled == true;

        internal void StartBgm()
        {
            if (!IsEnabledAndReady) return;

            string fileName = Music.Get() == BAGachaBgmChoice.@new ? "Theme_338.wav" : "Connected_Sky.wav";
            try
            {
                string path = ExtractResource(fileName);
                BgmAudioPlayer.Play(path, Volume.Get() / 100f);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start {Name} BGM", ex);
            }
        }

        internal void StopBgm()
        {
            BgmAudioPlayer.Stop();
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
            StopBgm();
        }

        public override void Enable()
        {
            var init = AccessTools.Method(typeof(nel.UiReelManager), "InitUiReelManager");
            var deactivate = AccessTools.Method(typeof(nel.UiReelManager), "deactivate");
            if (init == null || deactivate == null)
            {
                Log.Error($"{Name}: required UiReelManager methods were not found.");
                return;
            }

            _harmony.Patch(init, postfix: new HarmonyMethod(typeof(ModuleBAGachaBGM), nameof(InitPostfix)));
            _harmony.Patch(deactivate, postfix: new HarmonyMethod(typeof(ModuleBAGachaBGM), nameof(DeactivatePostfix)));
        }

        private static void InitPostfix()
        {
            Log.Debug($"{_instance.Name}: InitPostfix called.");
            if (IsEnabledAndReady) _instance.StartBgm();
        }

        private static void DeactivatePostfix()
        {
            Log.Debug($"{_instance.Name}: DeactivatePostfix called.");
            _instance?.StopBgm();
        }

        private static string ExtractResource(string fileName)
        {
            Directory.CreateDirectory(AudioFolder);
            string outputPath = Path.Combine(AudioFolder, fileName);
            Assembly assembly = typeof(ModuleBAGachaBGM).Assembly;
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(ResourcePrefix + fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
                throw new FileNotFoundException($"Embedded BGM resource not found: {fileName}");

            using (Stream input = assembly.GetManifestResourceStream(resourceName))
            using (FileStream output = File.Create(outputPath))
            {
                input.CopyTo(output);
            }
            return outputPath;
        }
    }

    internal static class BgmAudioPlayer
    {
        private static readonly object AudioLock = new();
        private static WaveOutEvent _outputDevice;
        private static WaveStream _audioStream;

        internal static void Play(string path, float volume)
        {
            Stop();

            WaveOutEvent outputDevice = null;
            WaveStream audioStream = null;
            try
            {
                var audioFileReader = new AudioFileReader(path)
                {
                    Volume = volume < 0f ? 0f : volume > 1f ? 1f : volume
                };
                audioStream = new LoopStream(audioFileReader);
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioStream);
                outputDevice.PlaybackStopped += (sender, args) => Stop();

                lock (AudioLock)
                {
                    _audioStream = audioStream;
                    _outputDevice = outputDevice;
                }

                outputDevice.Play();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to play BGM '{path}' with NAudio", ex);
                outputDevice?.Dispose();
                audioStream?.Dispose();
                Stop();
            }
        }

        internal static void Stop()
        {
            WaveOutEvent outputDevice;
            WaveStream audioStream;
            lock (AudioLock)
            {
                outputDevice = _outputDevice;
                audioStream = _audioStream;
                _outputDevice = null;
                _audioStream = null;
            }
            outputDevice?.Stop();
            outputDevice?.Dispose();
            audioStream?.Dispose();
        }

        private sealed class LoopStream : WaveStream
        {
            private readonly WaveStream _source;

            internal LoopStream(WaveStream source)
            {
                _source = source;
            }

            public override WaveFormat WaveFormat => _source.WaveFormat;

            public override long Length => _source.Length;

            public override long Position
            {
                get => _source.Position;
                set => _source.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalRead = 0;
                while (totalRead < count)
                {
                    int read = _source.Read(buffer, offset + totalRead, count - totalRead);
                    if (read == 0)
                    {
                        if (_source.Position == 0) break;
                        _source.Position = 0;
                        continue;
                    }
                    totalRead += read;
                }
                return totalRead;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _source.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
