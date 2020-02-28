using System;
using Newtonsoft.Json;

namespace Main
{

    internal sealed class ConfigJson : JsonClass<ConfigJson>
    {
        private static ConfigJson _instance;

        /// <summary>
        /// Singleton pattern
        /// </summary>
        public static ConfigJson Instance
        {
            get
            {
                if (_instance == null)
                    _instance = ConfigJson.Load("config.json");
                return _instance;

            }
        }


        [JsonProperty("token")]
        public string Token { get; private set; } = string.Empty;

        [JsonProperty("prefix")]
        public string[] CommandPrefix { get; private set; } = new[] { "d#", "d#+" };

        [JsonProperty("shards")]
        public int ShardCount { get; private set; } = 1;

        [JsonProperty("twitchToken")]
        public string TwitchToken { get; private set; } = "";


        public override void PostCreateFile()
        {
            Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();
        }
    }
}