# Unicord modern protocol backports

This branch selectively backports Discord protocol features needed by Unicord while keeping the old `netstandard2.0` / C# 10 base intact.

The goal is **not** to turn this fork into current DSharpPlus wholesale. Each feature is kept small enough to review and test independently.

## 1. Primary guild / server tags

Discord user payload:

```json
{
  "primary_guild": {
    "identity_guild_id": "123",
    "identity_enabled": true,
    "tag": "RAT",
    "badge": "badge_hash"
  }
}
```

Flow in this fork:

```text
JSON user payload
    -> TransportUser.PrimaryGuild
    -> DiscordUser.PrimaryGuild
    -> Unicord UserViewModel / UI
```

`DiscordUserPrimaryGuild.BadgeUrl` constructs the Discord CDN URL locally from the guild ID and badge hash. It does not trust an arbitrary URL from the gateway payload.

Consumers should only display a tag when `IdentityEnabled == true` and `Tag` is non-empty.

## 2. Polls

Poll data is modeled as part of `DiscordMessage`, matching Discord's API shape:

```text
DiscordMessage
    -> Poll?
        -> Question
        -> Answers[]
        -> Expiry
        -> AllowMultiselect
        -> Layout
        -> Results?
```

`DiscordMessage.Poll == null` means the message does not contain a poll.

`DiscordPoll.Results == null` means **results are unknown**, not zero votes. Discord explicitly allows the results object to be omitted from some payloads, so callers must preserve that distinction.

Result answer IDs primarily use the current API field `id`. The backport also accepts the older/upstream-model alias `answer_id` for compatibility.

## 3. Incremental poll vote updates

Current DSharpPlus understands:

- `MESSAGE_POLL_VOTE_ADD`
- `MESSAGE_POLL_VOTE_REMOVE`

The legacy Unicord dispatcher predates those event names and routes them through `UnknownEvent`. To avoid replacing the large legacy dispatch file in this first backport, `DiscordClient.Polls.cs` installs a narrow compatibility bridge when something subscribes to `MessagePollVoted`.

Only those two exact event names are converted.

```text
Gateway
  -> UnknownEvent (legacy dispatcher)
  -> poll bridge
  -> DiscordPollVoteUpdate
  -> MessagePollVoted
```

The bridge never performs a REST request. If the associated poll message and its result set are already cached, the affected answer count is updated in place. If results are unknown, the bridge leaves them unknown instead of manufacturing state.

This bridge is intentionally temporary. A future cleanup can add the two event names directly to `DiscordClient.Dispatch.cs` and remove the compatibility layer.

## 4. Message types

No `MessageType` backport was performed because the Unicord fork already contains the modern client-side values needed by the app, including:

- `ThreadStarterMessage = 21`
- AutoMod / role subscription types
- `PollResult = 46`
- newer client notification types through 48

Overwriting that enum with current upstream DSharpPlus would actually remove client-specific values that Unicord already understands.

## Safety boundaries

This branch intentionally does **not** add an undocumented user-account endpoint for casting poll votes.

The official Discord poll resource allows applications to read poll state and receive gateway updates, but applications are not allowed to cast votes. Unicord can safely render polls and react to vote events from the typed data model without teaching this library to perform undocumented authenticated actions.

Likewise, the backport does not make additional REST calls merely to decorate the UI. Guild tags and poll state are rendered from payload/cache data already received from Discord.

## What Unicord can consume now

```csharp
var tag = message.Author.PrimaryGuild?.Tag;
var poll = message.Poll;

client.MessagePollVoted += async (_, e) =>
{
    var answerId = e.PollVoteUpdate.AnswerId;
    var wasAdded = e.PollVoteUpdate.WasAdded;
};
```

The next application-layer work belongs in Unicord itself:

1. a reusable Fluent guild-tag control;
2. a poll view model and Fluent poll card;
3. handling `ThreadStarterMessage` as a referenced normal message instead of an unknown system message;
4. eventually moving the poll vote cases into the main gateway dispatcher when that file is cleaned up.
