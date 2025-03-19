using System.Collections.Generic;

namespace InteractionFramework.Jsons
{
    public class KillEvent
    {
        public int numberOfParticipants { get; set; }
        public int groupMemberCount { get; set; }
        public int EventId { get; set; }
        public string TimeStamp { get; set; }
        public Killer Killer { get; set; }
        public Victim Victim { get; set; }
        public int TotalVictimKillFame { get; set; }
        public List<Participant> Participants { get; set; }
        public object GvGMatch { get; set; }
        public int BattleId { get; set; }
        public string KillArea { get; set; }
        public string Type { get; set; }
    }

    public class Killer
    {
        public double AverageItemPower { get; set; }
        public Equipment Equipment { get; set; }
        public string Name { get; set; }
        public string GuildName { get; set; }
        public string AllianceName { get; set; }
    }

    public class Victim
    {
        public double AverageItemPower { get; set; }
        public Equipment Equipment { get; set; }
        public string Name { get; set; }
        public string GuildName { get; set; }
        public string AllianceName { get; set; }
        public List<Item> Inventory { get; set; }
    }

    public class Participant
    {
        public double AverageItemPower { get; set; }
        public string Name { get; set; }
        public string AllianceName { get; set; }
        public string GuildName { get; set; }
        public double DamageDone { get; set; }
        public double SupportHealingDone { get; set; }
    }

    public class Equipment
    {
        public Item MainHand { get; set; }
        public Item OffHand { get; set; }
        public Item Head { get; set; }
        public Item Armor { get; set; }
        public Item Shoes { get; set; }
        public Item Bag { get; set; }
        public Item Cape { get; set; }
        public Item Mount { get; set; }
        public Item Potion { get; set; }
        public Item Food { get; set; }
    }

    public class Item
    {
        public string Type { get; set; }

        public int Quality { get; set; }

        public int Count { get; set; }
    }
}
