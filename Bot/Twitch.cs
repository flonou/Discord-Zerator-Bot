using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Main
{
    public class Twitch : BaseCommandModule
    {

        private static JsonClass<StreamerStatus> streamerStatuses;
        public static JsonClass<StreamerStatus> StreamerStatuses
        {
            get
            {
                if (streamerStatuses == null)
                    streamerStatuses = JsonClass<StreamerStatus>.Load("twitch.json");
                return streamerStatuses;
            }
        }

        private static string token;

        private static string Token
        {
            get
            {
                if (token == null)
                {
                    token = JsonConfig.Instance.Data.TwitchToken;
                }
                return token;
            }
        }


        // thread checkant les lives contenu dans la config
        public static async Task CheckLive(DiscordClient client)
        {
            //int[] stream = { 28575692, 89872865, 18887776 };
            while (true)
            {
                foreach (Status id in StreamerStatuses.Data.Status)
                {
                    await WebRequestAsync(id, client);
                    //Console.WriteLine(string.Format("Response: {0} et {1}", id.Id, id.IsLive));
                }
                System.Threading.Thread.Sleep(300000);
            }
        }

        //check et affichage par channel des lives
        private static async Task WebRequestAsync(Status id, DiscordClient client)
        {
            Random rnd = new Random();
            string WEBSERVICE_URL = "https://api.twitch.tv/kraken/streams/" + id.Id;
            // string WEBSERVICE_URL = "https://api.twitch.tv/kraken/users?login=moman";
            try
            {
                var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 12000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Client-ID", Token);
                    webRequest.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");

                    using System.IO.Stream s = webRequest.GetResponse().GetResponseStream();
                    using System.IO.StreamReader sr = new System.IO.StreamReader(s);
                    var jsonResponse = sr.ReadToEnd();
                    //Console.WriteLine(string.Format("Response: {0}", jsonResponse));
                    if (jsonResponse.Contains("display_name"))
                    {
                        var twitch = JsonConvert.DeserializeObject<Streamer>(jsonResponse);
                        //return string.Format("Response: {0}", jsonResponse);
                        if (!id.IsLive)
                        {
                            Console.WriteLine(string.Format($"Live {id.Name} up!"));
                            // wrap it into an embed
                            var embed = new DiscordEmbedBuilder
                            {
                                Title = twitch.Stream.Channel.Display_name,
                                Description = twitch.Stream.Channel.Status,
                                Url = twitch.Stream.Channel.Url,
                                ImageUrl = twitch.Stream.Preview.Medium + rnd.Next(999999),
                                ThumbnailUrl = twitch.Stream.Channel.Logo
                            };
                            DiscordEmbedBuilder.EmbedFooter foot = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Joue à: " + twitch.Stream.Channel.Game,
                                IconUrl = "https://puush.poneyy.fr/TDaq.png"
                            };
                            embed.Footer = foot;
                            DiscordChannel info;
                            foreach (ulong channel in id.Channels)
                            {
                                info = await client.GetChannelAsync(channel);
                                await info.SendMessageAsync(embed: embed);
                            }
                            /* DiscordChannel info = await client.GetChannelAsync(channel);
                             await info.SendMessageAsync(embed: embed);*/
                            StreamerStatuses.Data.Status.First(d => d.Id == id.Id).IsLive = true;
                            await StreamerStatuses.Save();
                            client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"{id} en live", DateTime.Now);
                        }
                        else client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"{id} en live", DateTime.Now);
                    }
                    else if (id.IsLive)
                    {
                        // wrap it into an embed
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = id.Name,
                            Description = "Fin du live:wave: :wave:",
                            Url = "https://zerator.com/programmation"
                        };
                        DiscordEmbedBuilder.EmbedFooter foot = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = "Consultez la programmation pour plus d'informations!",
                            IconUrl = "https://puush.poneyy.fr/TDaq.png"
                        };
                        embed.Footer = foot;
                        DiscordChannel info;
                        /*info = await client.GetChannelAsync(id.Channels[0]);
                        await info.SendMessageAsync(embed: embed);*/
                        foreach (ulong channel in id.Channels)
                        {
                            info = await client.GetChannelAsync(channel);
                            await info.SendMessageAsync(embed: embed);
                        }

                        StreamerStatuses.Data.Status.First(d => d.Id == id.Id).IsLive = false;
                        await StreamerStatuses.Save();
                        client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"{id} plus en live", DateTime.Now);
                    }
                    //else Console.WriteLine($"{id} pas en live");
                    else client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"{id} pas en live", DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        [Group("twitch")] // this makes the class a group, but with a twist; the class now needs an ExecuteGroupAsync method
        [Description("twitch commands")]
        [RequirePermissions(Permissions.BanMembers)] // and restrict this to users who have appropriate permissions
        public class TwitchCommands
        {
            [Command("getidtwitch"), Description("Get the ID from an user")] // this will be displayed to tell users what this command does when they invoke help
            public async Task GetId(CommandContext ctx, [Description("Twitch user's name")] string user)
            {
                //string WEBSERVICE_URL = "https://api.twitch.tv/kraken/streams/" + id.Id;
                var WEBSERVICE_URL = $"https://api.twitch.tv/kraken/users?login={user}";
                try
                {
                    var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                    if (webRequest != null)
                    {
                        webRequest.Method = "GET";
                        webRequest.Timeout = 12000;
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("Client-ID", "0r570obyz0in1a85pqv16as54sfce1");
                        webRequest.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");

                        using System.IO.Stream s = webRequest.GetResponse().GetResponseStream();
                        using System.IO.StreamReader sr = new System.IO.StreamReader(s);
                        var jsonResponse = sr.ReadToEnd();
                        Console.WriteLine(string.Format("Response: {0}", jsonResponse));
                        await ctx.RespondAsync($"```{jsonResponse}```");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            [Command("add"), Description("add twitch live on this channel")]
            public async Task Add(CommandContext ctx, [Description("user's twitch name")] string name)
            {
                await ctx.TriggerTypingAsync();

                string WEBSERVICE_URL = $"https://api.twitch.tv/kraken/users?login={name}";
                try
                {
                    var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                    if (webRequest != null)
                    {
                        webRequest.Method = "GET";
                        webRequest.Timeout = 12000;
                        webRequest.ContentType = "application/json";
                        webRequest.Headers.Add("Client-ID", "0r570obyz0in1a85pqv16as54sfce1");
                        webRequest.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");

                        using System.IO.Stream s = webRequest.GetResponse().GetResponseStream();
                        using System.IO.StreamReader sr = new System.IO.StreamReader(s);
                        var jsonResponse = sr.ReadToEnd();
                        var twitch = JsonConvert.DeserializeObject<NameRequest>(jsonResponse);
                        int id = int.Parse(twitch.Users[0].Id);
                        //Console.WriteLine(string.Format("Response: {0}", jsonResponse));
                        if (StreamerStatuses.Data.Status.Any(prod => prod.Id == id))
                        {
                            int idx = StreamerStatuses.Data.Status.FindIndex(prod => prod.Id == id);
                            StreamerStatuses.Data.Status[idx].Channels.Add(ctx.Channel.Id);
                            await ctx.RespondAsync($"Notification de live de {name} ajoutée a ce channel.");
                        }
                        else
                        {
                            Status stream = new Status(name, false, id, ctx.Channel.Id);
                            StreamerStatuses.Data.Status.Add(stream);
                            await ctx.RespondAsync($"Notification de live de {name} ajoutée a ce channel.");
                        }
                        await StreamerStatuses.Save();
                    }
                }
                catch
                {
                    await ctx.RespondAsync("User twitch inexistant.");
                }
            }
            [Command("del"), Aliases("delete"), Description("delete twitch live on this channel")]
            public async Task Del(CommandContext ctx, [Description("user's twitch name")] string name)
            {
                await ctx.TriggerTypingAsync();
                if (StreamerStatuses.Data.Status.Any(prod => prod.Name == name))
                {
                    int idx = StreamerStatuses.Data.Status.FindIndex(prod => prod.Name == name);
                    if (StreamerStatuses.Data.Status[idx].Channels.Contains(ctx.Channel.Id))
                    {
                        StreamerStatuses.Data.Status[idx].Channels.Remove(ctx.Channel.Id);
                        if (StreamerStatuses.Data.Status[idx].Channels.Count == 0)
                        {
                            StreamerStatuses.Data.Status.RemoveAt(idx);
                            await ctx.RespondAsync($"Notification de stream de {name} supprimée du serveur (dernier channel notifié supprimé).");
                        }
                        else
                            await ctx.RespondAsync($"Notification de stream de {name} supprimée de ce channel.");

                    }
                    else
                        await ctx.RespondAsync($"Ce channel n'est pas notifié par ce live.");

                    await StreamerStatuses.Save();

                }


            }
        }
    }

    public class Preview
    {
        public string Small { get; set; }
        public string Medium { get; set; }
        public string Large { get; set; }
        public string Template { get; set; }
    }

    public class Channel
    {
        public bool Mature { get; set; }
        public string Status { get; set; }
        public string Broadcaster_language { get; set; }
        public string Broadcaster_software { get; set; }
        public string Display_name { get; set; }
        public string Game { get; set; }
        public string Language { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public bool Partner { get; set; }
        public string Logo { get; set; }
        public string Video_banner { get; set; }
        public string Profile_banner { get; set; }
        public string Profile_banner_background_color { get; set; }
        public string Url { get; set; }
        public int Views { get; set; }
        public int Followers { get; set; }
        public string Broadcaster_type { get; set; }
        public string Description { get; set; }
        public bool Private_video { get; set; }
        public bool Privacy_options_enabled { get; set; }
    }

    public class Stream
    {
        public long Id { get; set; }
        public string Game { get; set; }
        public string Broadcast_platform { get; set; }
        public string Community_id { get; set; }
        public List<object> Xommunity_ids { get; set; }
        public int Viewers { get; set; }
        public int Video_height { get; set; }
        public double Average_fps { get; set; }
        public int Delay { get; set; }
        public DateTime Created_at { get; set; }
        public bool Is_playlist { get; set; }
        public string Stream_type { get; set; }
        public Preview Preview { get; set; }
        public Channel Channel { get; set; }
    }

    public class Streamer
    {
        public Stream Stream { get; set; }
    }
    public class Status : IEquatable<Status>
    {
        public string Name { get; set; }
        public bool IsLive { get; set; }
        public int Id { get; set; }
        public List<ulong> Channels { get; set; }

        public Status(string Nam, bool IsLiv, int i, ulong chan = 334793139661570058)
        {
            Name = Nam;
            IsLive = IsLiv;
            Id = i;
            Channels = new List<ulong>
            {
                chan
            };
        }

        [JsonConstructor]
        public Status(string Nam, bool IsLiv, int i, List<ulong> chan)
        {
            Name = Nam;
            IsLive = IsLiv;
            Id = i;
            Channels = chan;
        }

        public override String ToString()
        {
            return Name;
        }

        public bool Equals(Status other)
        {
            // Would still want to check for null etc. first.
            return this.Id == other.Id;
        }
    }

    public class StreamerStatus
    {
        [JsonProperty("status")]
        public List<Status> Status { get; set; } = new List<Status>();


    }
    public class User
    {
        public string Display_name { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Bio { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public string Logo { get; set; }
    }

    public class NameRequest
    {
        public int Total { get; set; }
        public IList<User> Users { get; set; }
    }

}