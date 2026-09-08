using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using System.Text.RegularExpressions;

namespace Jarvis.Core.Skill
{
    public sealed partial class ReminderSkill(ReminderStore store, TimeProvider clock) : ISkill
    {
        public string Name => "reminder";
        public string Description => "Sets a reminder a fixed time from now.";
        public IReadOnlyList<string> Examples => ["remind me in 10 minutes to stretch", "list reminders"];

        // Source-generated regex: compiled at build time, no startup cost.
        [GeneratedRegex(@"remind me in (\d+)\s*(second|minute|hour)s?\s+to\s+(.+)", RegexOptions.IgnoreCase)]
        private static partial Regex SetPattern();

        public int Score(UserRequest request)
        {
            if (SetPattern().IsMatch(request.Normalized)) return 100;
            if (request.ContainsAnyWord("reminders") || request.ContainsAllWords("list reminder")) return 80;
            if (request.ContainsAnyWord("remind", "reminder")) return 40;
            return 0;
        }

        public Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var match = SetPattern().Match(request.Normalized);

            if (!match.Success)
                return Task.FromResult(ListReminders());

            var amount = int.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value.ToLowerInvariant();
            var text = match.Groups[3].Value.Trim();

            var delay = unit switch
            {
                "second" => TimeSpan.FromSeconds(amount),
                "minute" => TimeSpan.FromMinutes(amount),
                "hour" => TimeSpan.FromHours(amount),
                _ => TimeSpan.FromMinutes(amount)
            };

            var dueAt = clock.GetUtcNow() + delay;
            store.Add(text, dueAt);

            var confirmation = $"Okay - I'll remind you to {text} in {amount} {unit}{(amount == 1 ? "" : "s")}.";
            var spokenConfirmation = $"Sure Syed, I'll remind you to {text} in {amount} {unit}.";
            return Task.FromResult(SkillResult.Ok(confirmation, spokenConfirmation));
        }

        private SkillResult ListReminders()
        {
            var pending = store.Pending();

            if (pending.Count == 0)
                return SkillResult.Ok("You have no reminders set.");

            var now = clock.GetUtcNow();
            var lines = pending.Select(r =>
            {
                var left = r.DueAt - now;
                return $"  - {r.Text} (in {(int)left.TotalMinutes}m {left.Seconds}s)";
            });

            return SkillResult.Ok(
                $"{pending.Count} reminder(s):\n" + string.Join('\n', lines),
                $"You have {pending.Count} reminder{(pending.Count == 1 ? "" : "s")}.");
        }
    }
}
