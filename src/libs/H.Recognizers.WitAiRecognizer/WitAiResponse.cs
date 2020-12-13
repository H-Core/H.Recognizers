using Newtonsoft.Json;

namespace H.Recognizers
{
    internal sealed class WitAiResponse
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}