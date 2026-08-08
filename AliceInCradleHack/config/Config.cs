using AliceInCradleHack.config.group;
using System.IO;

namespace AliceInCradleHack.config
{
    /// <summary>
    /// A root config backed by a JSON file managed by <see cref="ConfigSystem"/>.
    /// </summary>
    public class Config : ValueGroup
    {
        public Config(string name, string description = null) : base(name, description) { }

        /// <summary>
        /// The final JSON file of this config. Only valid after registration.
        /// </summary>
        public string JsonFile
        {
            get
            {
                ConfigSystem.EnsureRegistered(this);
                return Path.Combine(ConfigSystem.ConfigsFolder, Name.ToLowerInvariant() + ".json");
            }
        }

        /// <summary>
        /// Temp file used for atomic writes: written first, then renamed over <see cref="JsonFile"/>.
        /// </summary>
        public string JsonTmpFile
        {
            get
            {
                ConfigSystem.EnsureRegistered(this);
                return Path.Combine(ConfigSystem.ConfigsFolder, Name.ToLowerInvariant() + ".json.tmp");
            }
        }
    }
}
