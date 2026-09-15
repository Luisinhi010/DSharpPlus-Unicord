using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord poll attached to a message.
    /// </summary>
    public sealed class DiscordPoll
    {
        [JsonProperty("question")]
        public DiscordPollMedia Question { get; internal set; }

        [JsonProperty("answers")]
        public IReadOnlyList<DiscordPollAnswer> Answers { get; internal set; }

        [JsonProperty("expiry")]
        public DateTimeOffset? Expiry { get; internal set; }

        [JsonProperty("allow_multiselect")]
        public bool AllowMultiselect { get; internal set; }

        [JsonProperty("layout_type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPollLayoutType Layout { get; internal set; }

        /// <summary>
        /// Gets the currently known poll results. This is intentionally nullable:
        /// Discord may omit results from some payloads, which means "unknown", not
        /// "zero votes".
        /// </summary>
        [JsonProperty("results", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordPollResult Results { get; internal set; }

        internal DiscordPoll() { }
    }
}
