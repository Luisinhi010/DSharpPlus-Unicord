using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents media attached to a poll question or answer.
    /// </summary>
    public sealed class DiscordPollMedia
    {
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; internal set; }

        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordComponentEmoji Emoji { get; internal set; }

        internal DiscordPollMedia() { }
    }
}
