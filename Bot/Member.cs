using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Main
{
    public class MemberCheck
    {
        // thread checkant les lives contenu dans la config
        /************************* SEUL PARTIE A DECOMMENTER, 3.2.3 VERS 4.0 COMPATIBILITE ******/
        public static async Task CheckMember(DiscordClient client)
        {
            await Check(client);

        }



        //check et affichage par channel des lives
        private static async Task Check(DiscordClient client)
        {
            try
            {
                JsonClass<ListMembers> members = JsonClass<ListMembers>.Load("member.json");
                DiscordRole roleMembre = client.Guilds[138283154589876224].GetRole(361927682671378442);
                foreach (ulong id in members.Data.Members)
                {
                    try
                    {
                        DiscordMember member = await client.Guilds[138283154589876224].GetMemberAsync(id);

                        if (!member.Roles.Contains(roleMembre))
                        {
                            if ((DateTime.Now - member.JoinedAt) >= TimeSpan.FromMinutes(10))
                            {
                                await member.GrantRoleAsync(roleMembre);
                                members.Data.Members.Remove(id);
                            }
                        }
                        else members.Data.Members.Remove(id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                await members.Save();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /* [Command("forceupdate")] // let's define this method as a command
         [Description("Force Member's Rank update.")] // this will be displayed to tell users what this command does when they invoke help
         public async Task Forceupdate(CommandContext ctx)
         {
             try
             {
                 await ctx.Channel.SendMessageAsync("Force update des grades membre, ceci de prendre un certain temps. . .");
                 IReadOnlyList<DiscordMember> members = await ctx.Guild.GetAllMembersAsync();
                 DiscordRole roleMembre = ctx.Guild.Roles.First(d => d.Name == "Membre");
                 int i = 0;
                 foreach (DiscordMember member in members)
                 {
                     if (!member.Roles.Contains(roleMembre))
                     {
                         if ((DateTime.Now - member.JoinedAt) >= TimeSpan.FromMinutes(10))
                         {
                             await member.GrantRoleAsync(roleMembre);
                             i++;
                         }
                     }
                 }
                 await ctx.Channel.SendMessageAsync($"membres sur le serveur: {members.Count}, nombre de membres ajouté: {i}");
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.ToString());
             }
         }*/

        public class ListMembers
        {
            [JsonProperty("Members")]
            public List<ulong> Members { get; set; } = new List<ulong>();

        }
    }
}