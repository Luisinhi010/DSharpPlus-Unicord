using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the result set attached to a Discord poll.
    /// </summary>
    public sealed class DiscordPollResult
    {
        [JsonProperty("is_finalized")]
        public bool IsFinalized { get; internal set; }

        [JsonProperty("answer_counts")]
        public IReadOnlyList<DiscordPollAnswerCount> Results { get; internal set; }

        internal DiscordPollResult() { }
    }
}
