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

        [Command("greet"), Description("Says hi to specified user."), Aliases("sayhi", "say_hi")]
        public async Task Greet(CommandContext ctx, [Description("The user to say hi to.")] DiscordMember member) // this command takes a member as an argument; you can pass one by username, nickname, id, or mention
        {
            // note the [Description] attribute on the argument.
            // this will appear when people invoke help for the
            // command.

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // let's make the message a bit more colourful
            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");

            // and finally, let's respond and greet the user.
            await ctx.RespondAsync($"{emoji} Hello, {member.Mention}!");
        }

        [Command("sum"), Description("Sums all given numbers and returns said sum.")]
        public async Task SumOfNumbers(CommandContext ctx, [Description("Integers to sum.")] params int[] args)
        {
            // note the params on the argument. It will indicate
            // that the command will capture all the remaining arguments
            // into a single array

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // calculate the sum
            var sum = args.Sum();

            // and send it to the user
            await ctx.RespondAsync($"The sum of these numbers is {sum.ToString("#,##0")}");
        }
    }

    [Group("admin")] // let's mark this class as a command group
    [Description("Administrative commands.")] // give it a description for help purposes
    [Hidden] // let's hide this from the eyes of curious users
    [RequirePermissions(Permissions.ManageGuild)] // and restrict this to users who have appropriate permissions
    public class ExampleGrouppedCommands : BaseCommandModule
    {
        // all the commands will need to be executed as <prefix>admin <command> <arguments>

        // this command will be only executable by the bot's owner
        [Command("sudo"), Description("Run a command as another user."), Hidden, RequireOwner]
        public async Task SudoAsync(CommandContext ctx, DiscordUser user, [RemainingText] string content)
        {
            var cmd = ctx.CommandsNext.FindCommand(content, out var args);
            var fctx = ctx.CommandsNext.CreateFakeContext(user, ctx.Channel, content, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx).ConfigureAwait(false);
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
            var embed = new DiscordEmbedBuilder            {
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
        [Command("del"), Aliases("delete") ,Description("delete MAL list")]
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
            using(StreamWriter file = File.CreateText("configMAL.json"))
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


    [Group("memes")] // this makes the class a group, but with a twist; the class now needs an ExecuteGroupAsync method
    [Description("Contains some memes. When invoked without subcommand, returns a random one.")]
    [Aliases("copypasta")]
    public class ExampleExecutableGroup
    {

        // commands in this group need to be executed as 
        // <prefix>memes [command] or <prefix>copypasta [command]

        // this is the group's command; unlike with other commands, 
        // any attributes on this one are ignored, but like other
        // commands, it can take arguments
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            // let's give them a random meme
            var rnd = new Random();
            var nxt = rnd.Next(0, 2);

            switch (nxt)
            {
                case 0:
                    await Pepe(ctx);
                    return;

                case 1:
                    await NavySeal(ctx);
                    return;

                case 2:
                    await Kekistani(ctx);
                    return;
            }
        }

        [Command("pepe"), Aliases("feelsbadman"), Description("Feels bad, man.")]
        public async Task Pepe(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            // wrap it into an embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pepe",
                ImageUrl = "http://i.imgur.com/44SoSqS.jpg"
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("navyseal"), Aliases("gorillawarfare"), Description("What the fuck did you just say to me?")]
        public async Task NavySeal(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces. You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo.");
        }

        [Command("kekistani"), Aliases("kek", "normies"), Description("I'm a proud ethnic Kekistani.")]
        public async Task Kekistani(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("I'm a proud ethnic Kekistani. For centuries my people bled under Normie oppression. But no more. We have suffered enough under your Social Media Tyranny. It is time to strike back. I hereby declare a meme jihad on all Normies. Normies, GET OUT! RRRÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆ﻿");
        }

        // this is a subgroup; you can nest groups as much 
        // as you like
        [Group("mememan"), Hidden]
        public class MemeMan
        {
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Meme man",
                    ImageUrl = "http://i.imgur.com/tEmKtNt.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("ukip"), Description("The UKIP pledge.")]
            public async Task Ukip(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "UKIP pledge",
                    ImageUrl = "http://i.imgur.com/ql76fCQ.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("lineofsight"), Description("Line of sight.")]
            public async Task LOS(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Line of sight",
                    ImageUrl = "http://i.imgur.com/ZuCUnEb.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("art"), Description("Art.")]
            public async Task Art(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Art",
                    ImageUrl = "http://i.imgur.com/VkmmmQd.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("seeameme"), Description("When you see a meme.")]
            public async Task SeeMeme(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "When you see a meme",
                    ImageUrl = "http://i.imgur.com/8GD0hbZ.jpg"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("thisis"), Description("This is meme man.")]
            public async Task ThisIs(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "This is meme man",
                    ImageUrl = "http://i.imgur.com/57vDOe6.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("deepdream"), Description("Deepdream'd meme man.")]
            public async Task DeepDream(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Deep dream",
                    ImageUrl = "http://i.imgur.com/U666J6x.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("sword"), Description("Meme with a sword?")]
            public async Task Sword(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Meme with a sword?",
                    ImageUrl = "http://i.imgur.com/T3FMXdu.png"
                };
                await ctx.RespondAsync(embed: embed);
            }

            [Command("christmas"), Description("Beneath the christmas spike...")]
            public async Task ChristmasSpike(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                // wrap it into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Christmas spike",
                    ImageUrl = "http://i.imgur.com/uXIqUS7.png"
                };
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
