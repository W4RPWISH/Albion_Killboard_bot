using InteractionFramework.Jsons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace InteractionFramework.Utils
{
    public static class ImageCreator
    {   
        private static SolidBrush white = new SolidBrush(Color.White);
        private static Font mainFont = GetFont(26);
        private static Font smallFont = GetFont(16);

        public static Stream CreateImage(KillEvent killEvent, string type)
        {
            using (Image image = GetLayout(killEvent, type))
            {
                DrawStrings(killEvent, image);
                DrawEquipment(GetItems(killEvent.Killer.Equipment), false, image);
                DrawEquipment(GetItems(killEvent.Victim.Equipment), true, image);
                DrawInventory(killEvent, image);

                return image.ToMemoryStream();
            }
        }

        private static Image GetLayout(KillEvent killEvent, string type)
        {
            int inventorySize = killEvent.Victim.Inventory.FindAll(x => x != null).Count();

            string imageFilePath = string.Empty;

            if (inventorySize == 0)
            {
                imageFilePath = $"assets/layout/{type}/0.png";
            }
            else if (inventorySize <= 9)
            {
                imageFilePath = $"assets/layout/{type}/9.png";
            }
            else if (inventorySize <= 18)
            {
                imageFilePath = $"assets/layout/{type}/18.png";
            }
            else if (inventorySize <= 27)
            {
                imageFilePath = $"assets/layout/{type}/27.png";
            }
            else if (inventorySize <= 36)
            {
                imageFilePath = $"assets/layout/{type}/36.png";
            }
            else if (inventorySize <= 45)
            {
                imageFilePath = $"assets/layout/{type}/45.png";
            }
            else
            {
                imageFilePath = $"assets/layout/{type}/45.png";
            }

            return Image.Load(imageFilePath);
        }

        private static void DrawStrings(KillEvent killEvent, Image image)
        {
            string killerName = killEvent.Killer.Name;
            string killerGuildAndAlliance = string.IsNullOrEmpty(killEvent.Killer.AllianceName) ? killEvent.Killer.GuildName : $"[{killEvent.Killer.AllianceName}] {killEvent.Killer.GuildName}";

            string victimName = killEvent.Victim.Name;
            string victimGuildAndAlliance = string.IsNullOrEmpty(killEvent.Victim.AllianceName) ? killEvent.Victim.GuildName : $"[{killEvent.Victim.AllianceName}] {killEvent.Victim.GuildName}";

            string fame = $"{killEvent.TotalVictimKillFame / 1000}K";
            string time = GetTime(killEvent.TimeStamp);

            image.Mutate(tx => tx.DrawText(killerName, mainFont, white, GetMainTextOffset(15, killerName)));
            image.Mutate(tx => tx.DrawText(killerGuildAndAlliance, mainFont, white, GetMainTextOffset(55, killerGuildAndAlliance)));

            image.Mutate(tx => tx.DrawText(fame, mainFont, white, GetMainTextOffset(170, fame, 1)));
            image.Mutate(tx => tx.DrawText(time, mainFont, white, GetMainTextOffset(515, time, 1)));

            image.Mutate(tx => tx.DrawText(victimName, mainFont, white, GetMainTextOffset(15, victimName, 2)));
            image.Mutate(tx => tx.DrawText(victimGuildAndAlliance, mainFont, white, GetMainTextOffset(55, victimGuildAndAlliance, 2)));
        }

        private static void DrawEquipment(List<Item> items, bool victim, Image image)
        {
            int offset = 0;

            if (victim)
                offset = 721;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;

                DownloadItem(items[i]);

                Image temp = FromFile($"assets/items/{GetItemPath(items[i])}");

                switch (i)
                {
                    case 0:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 51, 109), 1));
                        break;

                    case 1:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 177, 109), 1));
                        break;

                    case 2:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 301, 109), 1));
                        break;

                    case 3:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 51, 233), 1));
                        break;

                    case 4:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 177, 233), 1));
                        break;

                    case 5:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 301, 233), 1));
                        break;

                    case 6:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 51, 358), 1));
                        image.Mutate(x => x.DrawText(items[i].Count.ToString(), smallFont, white, GetSmallTextOffset(new Point(offset + 51, 358), items[i].Count.ToString())));
                        break;

                    case 7:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 177, 358), 1));
                        break;

                    case 8:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 301, 358), 1));
                        image.Mutate(x => x.DrawText(items[i].Count.ToString(), smallFont, white, GetSmallTextOffset(new Point(offset + 301, 358), items[i].Count.ToString())));
                        break;

                    case 9:
                        image.Mutate(x => x.DrawImage(temp, new Point(offset + 177, 480), 1));
                        break;
                }
            }
        }

        private static void DrawInventory(KillEvent killEvent, Image image)
        {
            int x = 33;
            int y = 667;
            int j = 0;

            foreach (Item item in killEvent.Victim.Inventory)
            {
                if (item == null) continue;

                DownloadItem(item);

                if ((j != 0) && (j % 9) == 0)
                {
                    x = 33;
                    y += 125;
                }

                image.Mutate(img => img.DrawImage(FromFile($"assets/items/{GetItemPath(item)}"), new Point(x, y), 1));
                image.Mutate(tx => tx.DrawText(item.Count.ToString(), smallFont, white, GetSmallTextOffset(new Point(x, y), item.Count.ToString())));

                x += 126;
                j++;
            }
        }

        private static void DownloadItem(Item item)
        {
            if (item.Quality > 1)
            {
                if (!File.Exists($"assets/items/{item.Type}_{item.Quality}.png"))
                {
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            client.DownloadFile(new Uri($"https://render.albiononline.com/v1/item/{item.Type}?quality={item.Quality}"), $"assets/items/{item.Type}_{item.Quality}.png");
                        }
                        catch { }
                    }
                }
            }
            else
            {
                if (!File.Exists($"assets/items/{item.Type}.png"))
                {
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            client.DownloadFile(new Uri($"https://render.albiononline.com/v1/item/{item.Type}"), $"assets/items/{item.Type}.png");
                        }
                        catch { }
                    }
                }
            }
        }

        private static Point GetSmallTextOffset(Point point, string text)
        {
            return new Point((int)(point.X - TextMeasurer.MeasureSize(text, new TextOptions(smallFont)).Width / 2 + 99), point.Y + 87);
        }

        private static Point GetMainTextOffset(int y, string text, int side = 0)
        {
            switch (side)
            {
                default://478
                    return new Point((int)(239 - TextMeasurer.MeasureSize(text, new TextOptions(mainFont)).Width / 2), y);

                case 1://242
                    return new Point((int)(599 - TextMeasurer.MeasureSize(text, new TextOptions(mainFont)).Width / 2), y);

                case 2://478
                    return new Point((int)(959 - TextMeasurer.MeasureSize(text, new TextOptions(mainFont)).Width / 2), y);
            }
        }

        private static string GetTime(string timestamp)
        {
            DateTime timeSpan = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(timestamp), TimeZoneInfo.Local);
            return $"{timeSpan.ToShortTimeString()} {timeSpan.Day} {GetMonth(timeSpan.Month)} {timeSpan.Year}";
        }

        private static string GetMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "Jan";

                case 2:
                    return "Feb";

                case 3:
                    return "Mar";

                case 4:
                    return "Apr";

                case 5:
                    return "May";

                case 6:
                    return "Jun";

                case 7:
                    return "Jul";

                case 8:
                    return "Aug";

                case 9:
                    return "Sep";

                case 10:
                    return "Oct";

                case 11:
                    return "Nov";
    
                default:
                    return "Dec";
            }
        }

        private static List<Item> GetItems(Equipment equipment)
        {
            List<Item> items = new List<Item>() { null, null, null, null, null, null, null, null, null, null };

            if (equipment.Bag != null)
                items[0] = equipment.Bag;

            if (equipment.Head != null)
                items[1] = equipment.Head;

            if (equipment.Cape != null)
                items[2] = equipment.Cape;

            if (equipment.MainHand != null)
                items[3] = equipment.MainHand;

            if (equipment.Armor != null)
                items[4] = equipment.Armor;

            if (equipment.OffHand != null)
                items[5] = equipment.OffHand;

            if (equipment.Food != null)
                items[6] = equipment.Food;

            if (equipment.Shoes != null)
                items[7] = equipment.Shoes;

            if (equipment.Potion != null)
                items[8] = equipment.Potion;

            if (equipment.Mount != null)
                items[9] = equipment.Mount;

            return items;
        }

        private static string GetItemPath(Item item)
        {
            return item.Quality > 1 ? $"{item.Type}_{item.Quality}.png" : $"{item.Type}.png";
        }

        private static Image FromFile(string path)
        {
            Image temp = null;

            try
            {
                temp = Image.Load(path);
            }
            catch 
            {
                File.Delete(path);
                temp = Image.Load("assets/items/T8_TRASH.png");
            }

            temp.Mutate(x => x.Resize(130, 130));

            return temp;
        }

        private static MemoryStream ToMemoryStream(this Image b)
        {
            MemoryStream ms = new MemoryStream();
            b.Save(ms, b.Metadata.DecodedImageFormat);

            return ms;
        }

        private static Font GetFont(int size)
        {
            FontCollection collection = new();
            FontFamily family = collection.Add("font/BakbakOne-Regular.ttf");
            Font font = family.CreateFont(size, FontStyle.Regular);

            return font;
        }
    }
}
