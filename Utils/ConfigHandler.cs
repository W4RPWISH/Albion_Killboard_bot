using InteractionFramework.Jsons;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace InteractionFramework.Utils
{
    public sealed class ConfigHandler
    {
        private readonly object locker = new object();

        #region SINGLETON

        private static ConfigHandler source = null;
        private static readonly object threadlock = new object();

        public static ConfigHandler Source
        {
            get
            {
                lock (threadlock)
                {
                    if (source == null)
                        source = new ConfigHandler();

                    return source;
                }
            }
        }

        #endregion

        public Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        public void Save()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public Dictionary<ulong, GuildSetting> GetData()
        {
            lock (locker)
            {
                return new Dictionary<ulong, GuildSetting>(config.GuildSettings);
            }
        }
    }
}
