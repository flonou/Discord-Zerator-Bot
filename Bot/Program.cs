using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using static Main.MemberCheck;

namespace Main
{
    internal sealed class Program
    {
        public static CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();
        private static CancellationToken CancelToken => CancelTokenSource.Token;

        private static List<Bot> Shards { get; } = new List<Bot>();


        public static void Main()
            => RunBotAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task RunBotAsync()
        {
            Console.Title = "Zerator Discord";

            var cfg = new ConfigJson();
            string json;
            if (!File.Exists("config.json"))
            {
                json = JsonConvert.SerializeObject(cfg);
                File.WriteAllText("config.json", json, new UTF8Encoding(false));
                Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
                Console.ReadKey();

                return;
            }
            if (!File.Exists("member.json"))
            {
                var cfgProg = new ListMembers();
                json = JsonConvert.SerializeObject(cfgProg);
                File.WriteAllText("member.json", json, new UTF8Encoding(false));
                Console.WriteLine("member file was not found, a new one was generated.");
            }
            if (!File.Exists("prog.json"))
            {
                var cfgProg = new ProgJson();
                json = JsonConvert.SerializeObject(cfgProg);
                File.WriteAllText("prog.json", json, new UTF8Encoding(false));
                Console.WriteLine("prog file was not found, a new one was generated.");
            }
            if (!File.Exists("twitch.json"))
            {
                var cfgTwitch = new StreamerStatus();
                json = JsonConvert.SerializeObject(cfgTwitch);
                File.WriteAllText("twitch.json", json, new UTF8Encoding(false));
                Console.WriteLine("twitch file was not found, a new one was generated.");
            }
            if (!File.Exists("configMAL.json"))
            {
                var cfgTwitch = new Dictionary<string, string>(new Dictionary<string, string>()); ;
                json = JsonConvert.SerializeObject(cfgTwitch);
                File.WriteAllText("configMAL.json", json, new UTF8Encoding(false));
                Console.WriteLine("configMAL file was not found, a new one was generated.");
            }
            json = File.ReadAllText("config.json", new UTF8Encoding(false));
            cfg = JsonConvert.DeserializeObject<ConfigJson>(json);


            var tskl = new List<Task>();
            for (var i = 0; i < cfg.ShardCount; i++)
            {
                var bot = new Bot(cfg, i);
                Shards.Add(bot);
                tskl.Add(bot.RunAsync());
                await Task.Delay(7500).ConfigureAwait(false);
            }

            await Task.WhenAll(tskl).ConfigureAwait(false);

            try
            {
                await Task.Delay(-1, CancelToken).ConfigureAwait(false);
            }
            catch (Exception) { /* shush */ }
        }
    }

    public class ProgJson
    {
        [JsonProperty("prog")]
        public string Prog { get; set; } = string.Empty;
    }
}