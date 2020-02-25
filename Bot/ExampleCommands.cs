using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Main
{
    public class ExampleUngrouppedCommands : BaseCommandModule
    {
        [Command("ping")] // let's define this method as a command
        [Description("Example ping command")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pong")] // alternative names for the command
        public async Task Ping(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // let's make the message a bit more colourful
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            // respond with ping
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
        }
    }

    [Group("MAL")] // this makes the class a group, but with a twist; the class now needs an ExecuteGroupAsync method
    [Description("My AnimeList")]
    [Aliases("MyAnimeList")]
    [RequirePermissions(Permissions.BanMembers)] // and restrict this to users who have appropriate permissions
    public class MyAnimeListCommands : BaseCommandModule
    {

        [Command("view"), Aliases("show", "list"), Description("View MAL list")]
        public async Task View(CommandContext ctx)
        {
            var cfgjson = LoadConfig();

            await ctx.TriggerTypingAsync();
            // wrap it into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "MyAnimeList",
                Description = "MyAnimeList des membres"
            };
            foreach (KeyValuePair<string, string> user in cfgjson) embed.Description += $"\n[{user.Key}]({user.Value})";
            await ctx.RespondAsync(embed: embed);
        }
        [Command("add"), Description("add MAL list")]
        public async Task Add(CommandContext ctx, [Description("Pseudo ")] string member, [RemainingText, Description("URL vers sa MAL")] string url)
        {
            var cfgjson = LoadConfig();

            await ctx.TriggerTypingAsync();
            if (cfgjson.ContainsKey(member))
            {
                cfgjson[member] = url;
                await ctx.RespondAsync($"MAL de {member} modifiée.");
            }
            else
            {
                cfgjson.Add(member, url);
                await ctx.RespondAsync($"MAL de {member} ajoutée.");
            }
            await SaveConfig(cfgjson);
        }
        [Command("del"), Aliases("delete"), Description("delete MAL list")]
        public async Task Del(CommandContext ctx, [Description("Pseudo ")] string member)
        {
            var cfgjson = LoadConfig();

            await ctx.TriggerTypingAsync();
            if (cfgjson.ContainsKey(member))
            {
                cfgjson.Remove(member);
                await ctx.RespondAsync($"MAL de {member} supprimé.");
                await SaveConfig(cfgjson);
            }
            else
            {
                await ctx.RespondAsync($"Aucune MAL associé a ce nom.");
            }
        }

        private Dictionary<string, string> LoadConfig()
        {
            // first, let's load our configuration file
            var json = File.ReadAllText("configMAL.json", new UTF8Encoding(false));
            // convert json to class
            var cfgjson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            // return
            return cfgjson;
        }
        private Task SaveConfig(Dictionary<string, string> cfgjson)
        {
            // first, let's load our configuration file

            //write string to file
            //await File.OpenWrite("configMAL.json");
            using (StreamWriter file = File.CreateText("configMAL.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, cfgjson);
            }
            // next, let's load the values from that file
            // to our client's configuration
            //var cfgjson = JsonConvert.DeserializeObject<ConfigMAL>(json);
            return Task.CompletedTask;
        }
    }


    [Group("prog")] // this makes the class a group, but with a twist; the class now needs an ExecuteGroupAsync method
    [Description("LA PROOOOOOOG")]
    [Aliases("programmation")]
    public class Programmation : BaseCommandModule
    {

        // commands in this group need to be executed as 
        // <prefix>memes [command] or <prefix>copypasta [command]

        // this is the group's command; unlike with other commands, 
        // any attributes on this one are ignored, but like other
        // commands, it can take arguments
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            ProgJson url = LoadConfig();
            var embed = new DiscordEmbedBuilder
            {
                Title = "Programation",
                Url = "http://zerator.com/programmation",
            };

            if (url.Prog != "") embed.ImageUrl = url.Prog;
            else embed.Description = "Programmation non disponible";
            await ctx.RespondAsync(embed: embed);
        }

        [Command("edit"), Aliases("modif"), Description("Changer la prog")]
        [RequirePermissions(Permissions.BanMembers)] // and restrict this to users who have appropriate permissions
        public async Task Edit(CommandContext ctx, [Description("lien vers l'image de la prog")] string newProg)
        {
            ProgJson url = LoadConfig();
            await ctx.TriggerTypingAsync();
            url.Prog = newProg;
            // wrap it into an embed
            await SaveConfig(url);

            await ctx.RespondAsync("Programmation mise à jour");
        }
        private static ProgJson LoadConfig()
        {
            // first, let's load our configuration file
            var json = File.ReadAllText("prog.json", new UTF8Encoding(false));
            // convert json to class
            var cfgjson = JsonConvert.DeserializeObject<ProgJson>(json);
            // return
            return cfgjson;
        }
        private Task SaveConfig(ProgJson cfgjson)
        {
            // first, let's load our configuration file

            //write string to file
            //await File.OpenWrite("configMAL.json");
            using (StreamWriter file = File.CreateText("prog.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, cfgjson);
            }
            // next, let's load the values from that file
            // to our client's configuration
            //var cfgjson = JsonConvert.DeserializeObject<ConfigMAL>(json);
            return Task.CompletedTask;
        }

    }
}
