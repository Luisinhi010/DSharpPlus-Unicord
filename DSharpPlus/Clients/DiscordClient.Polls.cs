using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    public sealed partial class DiscordClient
    {
        private AsyncEvent<DiscordClient, MessagePollVotedEventArgs> _messagePollVoted;

        /// <summary>
        /// Fired when a vote is added to or removed from a poll.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessagePollVotedEventArgs> MessagePollVoted
        {
            add
            {
                this.EnsurePollVoteEvent();
                this._messagePollVoted.Register(value);
            }
            remove => this._messagePollVoted?.Unregister(value);
        }

        private void EnsurePollVoteEvent()
        {
            this._messagePollVoted ??= new AsyncEvent<DiscordClient, MessagePollVotedEventArgs>(
                "MESSAGE_POLL_VOTED",
                this.EventErrorHandler);
        }

        /// <summary>
        /// Handles Discord's incremental poll vote gateway events.
        /// </summary>
        internal async Task OnMessagePollVoteUpdateAsync(JObject data, bool wasAdded)
        {
            if (data == null)
                return;

            DiscordPollVoteUpdate voteUpdate;
            try
            {
                voteUpdate = data.ToObject<DiscordPollVoteUpdate>();
            }
            catch (JsonException)
            {
                // A malformed gateway payload should not take down the dispatch loop.
                return;
            }

            if (voteUpdate == null)
                return;

            voteUpdate.WasAdded = wasAdded;
            voteUpdate.client = this;

            this.ApplyCachedPollVoteUpdate(voteUpdate);
            this.EnsurePollVoteEvent();

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
