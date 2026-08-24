using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace DSharpPlus
{
    public sealed partial class DiscordClient
    {
        private AsyncEvent<DiscordClient, MessagePollVotedEventArgs> _messagePollVoted;
        private bool _pollVoteUnknownEventBridgeRegistered;

        /// <summary>
        /// Fired when a vote is added to or removed from a poll.
        /// </summary>
        /// <remarks>
        /// The legacy Unicord fork predates MESSAGE_POLL_VOTE_ADD and
        /// MESSAGE_POLL_VOTE_REMOVE, so its main dispatch switch currently routes
        /// those payloads through UnknownEvent. This compatibility bridge converts
        /// only those two known event names into a typed event without modifying the
        /// large legacy dispatcher. It can be removed once the dispatcher itself is
        /// modernized.
        /// </remarks>
        public event AsyncEventHandler<DiscordClient, MessagePollVotedEventArgs> MessagePollVoted
        {
            add
            {
                this.EnsurePollVoteBridge();
                this._messagePollVoted.Register(value);
            }
            remove => this._messagePollVoted?.Unregister(value);
        }

        private void EnsurePollVoteBridge()
        {
            this._messagePollVoted ??= new AsyncEvent<DiscordClient, MessagePollVotedEventArgs>(
                "MESSAGE_POLL_VOTED",
                this.EventErrorHandler);

            if (this._pollVoteUnknownEventBridgeRegistered)
                return;

            this._unknownEvent.Register(this.OnPollVoteUnknownEventAsync);
            this._pollVoteUnknownEventBridgeRegistered = true;
        }

        private async Task OnPollVoteUnknownEventAsync(DiscordClient sender, UnknownEventArgs eventArgs)
        {
            if (eventArgs == null || string.IsNullOrWhiteSpace(eventArgs.EventName) || string.IsNullOrWhiteSpace(eventArgs.Json))
                return;

            var eventName = eventArgs.EventName.ToLowerInvariant();
            bool wasAdded;

            switch (eventName)
            {
                case "message_poll_vote_add":
                    wasAdded = true;
                    break;
                case "message_poll_vote_remove":
                    wasAdded = false;
                    break;
                default:
                    return;
            }

            DiscordPollVoteUpdate voteUpdate;
            try
            {
                voteUpdate = JsonConvert.DeserializeObject<DiscordPollVoteUpdate>(eventArgs.Json);
            }
            catch (JsonException)
            {
                // UnknownEvent may contain arbitrary future Discord payloads. Never let
                // a malformed poll payload take down the gateway event pipeline.
                return;
            }

            if (voteUpdate == null)
                return;

            voteUpdate.WasAdded = wasAdded;
            voteUpdate.client = this;

            this.ApplyCachedPollVoteUpdate(voteUpdate);

            await this._messagePollVoted.InvokeAsync(
                this,
                new MessagePollVotedEventArgs { PollVoteUpdate = voteUpdate })
                .ConfigureAwait(false);
        }

        private void ApplyCachedPollVoteUpdate(DiscordPollVoteUpdate voteUpdate)
        {
            var message = voteUpdate.Message;
            var results = message?.Poll?.Results?.Results;

            // Discord explicitly documents an omitted `results` object as unknown.
            // Do not manufacture a synthetic result set just because a vote event
            // arrived; preserve that distinction for callers.
            if (results == null)
                return;

            var answer = results.FirstOrDefault(result => result.AnswerId == voteUpdate.AnswerId);
            if (answer == null)
                return;

            if (voteUpdate.WasAdded)
                answer.Count++;
            else if (answer.Count > 0)
                answer.Count--;

            if (this.CurrentUser != null && voteUpdate.UserId == this.CurrentUser.Id)
                answer.SelfVoted = voteUpdate.WasAdded;
        }
    }
}
