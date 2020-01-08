using Newtonsoft.Json;

namespace Main
{

    internal sealed class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; } = string.Empty;

        [JsonProperty("prefix")]
        public string[] CommandPrefix { get; private set; } = new[] { "d#", "d#+" };

        [JsonProperty("shards")]
        public int ShardCount { get; private set; } = 1;

        [JsonProperty("twitchToken")]
        public string TwitchToken { get; private set; } = "";
    }
}