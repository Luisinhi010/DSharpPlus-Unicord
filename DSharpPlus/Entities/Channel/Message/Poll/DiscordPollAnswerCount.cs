using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the current result for one poll answer.
    /// </summary>
    public sealed class DiscordPollAnswerCount
    {
        private int _answerId;

        /// <summary>
        /// Gets the ID of this poll answer.
        /// </summary>
        [JsonProperty("id")]
        public int AnswerId
        {
            get => this._answerId;
            internal set => this._answerId = value;
        }

        // Some DSharpPlus versions modeled this field as answer_id. Accept it as a
        // compatibility alias, but prefer the current Discord API field name (id).
        [JsonProperty("answer_id")]
        private int LegacyAnswerId
        {
            set
            {
                if (this._answerId == 0)
                    this._answerId = value;
            }
        }

        [JsonProperty("count")]
        public int Count { get; internal set; }

        [JsonProperty("me_voted")]
        public bool SelfVoted { get; internal set; }

        internal DiscordPollAnswerCount() { }
    }
}
