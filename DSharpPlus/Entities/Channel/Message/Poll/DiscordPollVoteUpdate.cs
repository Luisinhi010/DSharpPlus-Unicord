using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a gateway update for a poll vote.
    /// </summary>
    public class DiscordPollVoteUpdate
    {
        internal DiscordClient client;

        /// <summary>
        /// Gets whether this vote was added or removed.
        /// </summary>
        [JsonIgnore]
        public bool WasAdded { get; internal set; }

        /// <summary>
        /// Gets the user involved in this vote update, using the local cache when available.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User
            => this.client?.GetCachedOrEmptyUserInternal(this.UserId);

        /// <summary>
        /// Gets the channel containing the poll, using the local cache when available.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => this.client?.InternalGetCachedChannel(this.ChannelId) ?? this.client?.InternalGetCachedThread(this.ChannelId);

        /// <summary>
        /// Gets the cached message containing the poll. No REST request is made.
        /// </summary>
        [JsonIgnore]
        public DiscordMessage Message
        {
            get
            {
                if (this.client?.MessageCache == null)
                    return null;

                return this.client.MessageCache.TryGet(
                    message => message.Id == this.MessageId && message.ChannelId == this.ChannelId,
                    out var cached)
                    ? cached
                    : null;
            }
        }

        /// <summary>
        /// Gets the guild containing this poll, if applicable and cached.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => this.GuildId.HasValue ? this.client?.InternalGetCachedGuild(this.GuildId) : null;

        [JsonProperty("user_id")]
        public ulong UserId { get; internal set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }

        [JsonProperty("message_id")]
        public ulong MessageId { get; internal set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? GuildId { get; internal set; }

        [JsonProperty("answer_id")]
        public int AnswerId { get; internal set; }

        internal DiscordPollVoteUpdate() { }
    }
}
