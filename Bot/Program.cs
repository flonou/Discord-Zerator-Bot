using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            var cfg = JsonConfig.Instance.Data;
            string json;

            if (!File.Exists("prog.json"))
            {
                var cfgProg = new ProgJson();
                json = JsonConvert.SerializeObject(cfgProg);
                File.WriteAllText("prog.json", json, new UTF8Encoding(false));
                Console.WriteLine("prog file was not found, a new one was generated.");
            }

            if (!File.Exists("configMAL.json"))
            {
                var cfgTwitch = new Dictionary<string, string>(new Dictionary<string, string>()); ;
                json = JsonConvert.SerializeObject(cfgTwitch);
                File.WriteAllText("configMAL.json", json, new UTF8Encoding(false));
                Console.WriteLine("configMAL file was not found, a new one was generated.");
            }


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