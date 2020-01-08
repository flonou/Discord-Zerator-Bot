// THIS FILE IS A PART OF EMZI0767'S BOT EXAMPLES
//
// --------
// 
// Copyright 2017 Emzi0767
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// --------
//
// This is a commands example. It shows how to properly utilize 
// CommandsNext, as well as use its advanced functionality.

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Main
{
    public class MemberCheck
    {
        // thread checkant les lives contenu dans la config
        /************************* SEUL PARTIE A DECOMMENTER, 3.2.3 VERS 4.0 COMPATIBILITE ******/
        /* public static async Task CheckMember(DiscordClient client)
         {
             await Check(client);

         }*/



        //check et affichage par channel des lives
        /*private static async Task Check(DiscordClient client)
        {
            try
            {
                ListMembers members = LoadConfig();
                DiscordRole roleMembre = client.Guilds[138283154589876224].Roles.I ( First(d => d.Key = "Membre");
                foreach (ulong id in members.members)
                {
                    try
                    {
                        DiscordMember member = await client.Guilds[138283154589876224].GetMemberAsync(id);

                        if (!member.Roles.Contains(roleMembre))
                        {
                            if ((DateTime.Now - member.JoinedAt) >= TimeSpan.FromMinutes(10))
                            {
                                await member.GrantRoleAsync(roleMembre);
                                members.members.Remove(id);
                            }
                        }
                        else members.members.Remove(id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
               await SaveConfig(members);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }*/

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

        private static ListMembers LoadConfig()
        {
            // first, let's load our configuration file
            var json = File.ReadAllText("member.json", new UTF8Encoding(false));
            // convert json to class
            var cfgjson = JsonConvert.DeserializeObject<ListMembers>(json);
            // return
            return cfgjson;
        }

        private static Task SaveConfig(ListMembers cfgjson)
        {
            // first, let's load our configuration file

            _ = JsonConvert.SerializeObject(cfgjson.ToString());

            //write string to file
            //await File.OpenWrite("configMAL.json");
            using (StreamWriter file = File.CreateText("member.json"))
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

        public class ListMembers
        {
            [JsonProperty("Members")]
            public List<ulong> Members { get; set; } = new List<ulong>();
        }
    }
}