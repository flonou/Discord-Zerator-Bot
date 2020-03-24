using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using static Main.MemberCheck;
using static Main.Twitch;

namespace Main
{
    internal sealed class Bot
    {
        public DiscordClient Client { get; set; }
        private Config Config { get; }
        private CommandsNextExtension CommandsNextService { get; }

        private static JsonClass<ListMembers> listMembers;
        public static JsonClass<ListMembers> ListMembers
        {
            get
            {
                if (listMembers == null)
                    listMembers = JsonClass<ListMembers>.Load("member.json");
                return listMembers;
            }
        }

        public Bot(Config cfg, int shardid)
        {
            // then we want to instantiate our client
            this.Config = cfg;

            // discord instance config and the instance itself
            var dcfg = new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = false,
                ShardId = shardid,
                ShardCount = this.Config.ShardCount,
                MessageCacheSize = 2048,
                DateTimeFormat = "dd-MM-yyyy HH:mm:ss zzz"
            };
            this.Client = new DiscordClient(dcfg);


            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;
            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = this.Config.CommandPrefix,

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            this.CommandsNextService = this.Client.UseCommandsNext(ccfg);
            this.CommandsNextService.CommandExecuted += this.Commands_CommandExecuted;
            this.CommandsNextService.CommandErrored += this.Commands_CommandErrored;
            this.Client.GuildBanAdded += this.Ban;
            this.Client.GuildBanRemoved += this.Unban;
            this.Client.GuildMemberAdded += this.NewMember;
            System.Threading.Thread liveChecker = new System.Threading.Thread(async e => await CheckLive(this.Client));
            liveChecker.Start();
            // let's add a converter for a custom type and a name

            //general (ping)
            this.CommandsNextService.RegisterCommands<ExampleUngrouppedCommands>();
            //MyAnimeList Commands
            //this.CommandsNextService.RegisterCommands<MyAnimeListCommands>();
            //Programmation
            //this.CommandsNextService.RegisterCommands<Programmation>();

            //Check member
            //this.Commands.RegisterCommands<MemberCheck>();
            /*  //Check nouveaux mambres
                        var memberTimer = new System.Threading.Timer(
                            async e => await CheckMember(this.Client), null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10));
            */
        }

        private async Task Unban(GuildBanRemoveEventArgs e)
        {
            if (e.Client != Client)
            {
                var emoji = DiscordEmoji.FromName(this.Client, ":dove:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Unban {emoji}",
                    Description = $"User {e.Client.CurrentUser.Username}.",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await e.Guild.GetChannel(334793139661570058).SendMessageAsync("", embed: embed);
            }
        }

        private Task NewMember(GuildMemberAddEventArgs e)
        {
            if (e.Client != Client)
            {

                ListMembers.Data.Members.Add(e.Client.CurrentUser.Id);
                ListMembers.Save();

            }
            return Task.CompletedTask;
        }
        private async Task Ban(GuildBanAddEventArgs e)
        {
            if (e.Client != Client)
            {
                var emoji = DiscordEmoji.FromName(this.Client, ":hammer:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"ban {emoji}",
                    Description = $"User {e.Client.CurrentUser.Username}.",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await e.Guild.GetChannel(334793139661570058).SendMessageAsync("", embed: embed);
            }
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", "Client is ready to process events.", DateTime.Now);
            Console.WriteLine($"{DateTime.Now} PoneyyBot: Client is ready to process events.");
            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"Guild available: {e.Guild.Name}", DateTime.Now);
            Console.WriteLine($"{DateTime.Now} PoneyyBot: Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "PoneyyBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            Console.WriteLine($"{DateTime.Now} PoneyyBot: Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }
        public async Task RunAsync()
        {
            var act = new DiscordActivity("!aled", ActivityType.ListeningTo);
            await Client.ConnectAsync(act, UserStatus.DoNotDisturb).ConfigureAwait(false);
        }
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "PoneyyBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "PoneyyBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }



    }
    // this structure will hold data from config.json
    /*   public struct ConfigJson
       {
           [JsonProperty("token")]
           public string Token { get; private set; }

           [JsonProperty("prefix")]
           public string CommandPrefix { get; private set; }
       }*/

}
