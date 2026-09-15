using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an answer in a Discord poll.
    /// </summary>
    public class DiscordPollAnswer
    {
        [JsonProperty("answer_id")]
        public int AnswerId { get; internal set; }

        [JsonProperty("poll_media")]
        public DiscordPollMedia AnswerData { get; internal set; }

        internal DiscordPollAnswer() { }
    }
}
