using AliceInCradleHack.utils.client;
using NAudio.Wave;
using System;
using System.IO;

namespace AliceInCradleHack.module
{
    /// <summary>
    /// Base class for modules that play a sound file through NAudio. Handles the audio
    /// lifecycle (load, play, dispose); subclasses decide when to play.
    /// </summary>
    public abstract class ModuleSoundBase : Module
    {
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFileReader;

        protected ModuleSoundBase(string name, string description, string category)
            : base(name, description, category)
        {
        }

        /// <summary>Playback volume factor (0-1), read each time a sound starts.</summary>
        protected abstract float VolumeFactor { get; }

        /// <summary>Plays the given sound file, stopping any sound that is still playing.</summary>
        protected void PlaySound(string soundFilePath)
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
                    Volume = VolumeFactor
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

        protected void DisposeAudio()
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
    }
}
