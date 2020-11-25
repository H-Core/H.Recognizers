using Newtonsoft.Json;

namespace H.Converters
{
    internal sealed class WitAiResponse
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}