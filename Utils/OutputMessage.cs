using Discord;
using Discord.WebSocket;
using InteractionFramework.Jsons;
using System.Linq;

namespace InteractionFramework.Utils
{
    public class OutputMessage
    {
        private static uint maincolor = 12648192;

        public static async void BroadcastMessage(SocketTextChannel channel, KillEvent killEvent, string type)
        {
            string message = $"[**{killEvent.Killer.Name} ({(int)killEvent.Killer.AverageItemPower})**](https://murderledger.com/players/{killEvent.Killer.Name}/ledger) :dagger: [**{killEvent.Victim.Name} ({(int)killEvent.Victim.AverageItemPower})**](https://murderledger.com/players/{killEvent.Victim.Name}/ledger) :skull_crossbones:";

            if (killEvent.Participants.Count() > 1)
            {
                message += "\n**Participants : **";

                killEvent.Participants.Remove(killEvent.Participants.Find(x => x.Name == killEvent.Killer.Name));

                foreach (Participant participant in killEvent.Participants)
                {
                    message += $"[**{participant.Name} ({(int)participant.AverageItemPower})**](https://murderledger.com/players/{participant.Name}/ledger)";

                    if (participant != killEvent.Participants.Last())
                    {
                        message += ", ";
                    }
                }
            }

            message += $"\n**Link : [albiononline.com](https://albiononline.com/killboard/kill/{killEvent.EventId})**";

            EmbedBuilder mainEmbed = new EmbedBuilder()
            {
                Title = "",
                Description = message,
                ImageUrl = "attachment://image.png",
                Color = maincolor,
            };

            try
            {
                await channel.SendFileAsync(ImageCreator.CreateImage(killEvent, type), "image.png", embed: mainEmbed.Build());
            }
            catch { }
        }
    }
}
