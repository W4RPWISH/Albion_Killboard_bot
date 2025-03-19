using System;
using System.Collections.Generic;

namespace InteractionFramework.Jsons
{
    public class Config
    {
        public Dictionary<ulong, GuildSetting> GuildSettings { get; set; }
    }

    public class GuildSetting
    {
        public bool LimitNotify { get; set; }

        public int FreeLimit { get; set; }

        public DateTime Subscription { get; set; }

        public MasterVoiceSetting MasterVoiceSetting { get; set; }

        public Dictionary<ulong, KillBotSetting> KillBotSettings { get; set; }
    }

    public class MasterVoiceSetting
    {
        public ulong MasterVoiceChannelId { get; set; }

        public ulong CategoryId { get; set; }
    }

    public class KillBotSetting
    {
        public FilterType Type { get; set; }

        public string Condition { get; set; }
    }

    public enum FilterType : int
    {
        Nickname,
        Guild,
        AllianceTag,
        All
    }
}
