using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a user's primary guild, which Discord displays as a server/guild tag.
    /// </summary>
    public class DiscordUserPrimaryGuild
    {
        internal DiscordUserPrimaryGuild() { }

        /// <summary>
        /// Gets the ID of the user's primary guild, if available.
        /// </summary>
        [JsonProperty("identity_guild_id")]
        public ulong? IdentityGuildId { get; internal set; }

        /// <summary>
        /// Gets whether the user is currently displaying the primary guild tag.
        /// </summary>
        [JsonProperty("identity_enabled")]
        public bool? IdentityEnabled { get; internal set; }

        /// <summary>
        /// Gets the guild tag text. Discord currently limits this to four characters.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; internal set; }

        /// <summary>
        /// Gets the guild tag badge hash.
        /// </summary>
        [JsonProperty("badge")]
        public string BadgeHash { get; internal set; }

        /// <summary>
        /// Gets the Discord CDN URL for the guild tag badge, when enough data is present.
        /// The URL is constructed locally from Discord-provided identifiers rather than
        /// accepting an arbitrary remote URL from the payload.
        /// </summary>
        [JsonIgnore]
        public string BadgeUrl
            => !this.IdentityGuildId.HasValue || string.IsNullOrWhiteSpace(this.BadgeHash)
                ? null
                : $"https://cdn.discordapp.com/guild-tag-badges/{this.IdentityGuildId.Value}/{this.BadgeHash}.png";
    }
}
