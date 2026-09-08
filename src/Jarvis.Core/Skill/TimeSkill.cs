using Jarvis.Core.Interface;
using Jarvis.Core.Service;

namespace Jarvis.Core.Skill
{
    public class TimeSkill(TimeProvider clock) : ISkill
    {
        public string Name => "time";

        public string Description => "Tells you the current time or date.";

        public IReadOnlyList<string> Examples => ["what time is it", "what's the date today"];

        public Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var now = clock.GetLocalNow();

            var wantsDate = request.ContainsAnyWord("date", "today", "day");
            var wantsTime = request.ContainsAnyWord("time", "clock");

            var text = (wantsDate, wantsTime) switch
            {
                (true, true) => $"It's {now:h:mm tt} on {now:dddd, d MMMM yyyy}.",
                (true, false) => $"Today is {now:dddd, d MMMM yyyy}.",
                _ => $"It's {now:h:mm tt}."
            };

            return Task.FromResult(SkillResult.Ok(text, text));

        }

        public int Score(UserRequest request)
        {
            var asksForTime = request.ContainsAnyWord("time", "clock");
            var asksForDate = request.ContainsAnyWord("date", "today", "day");
            var isQuestion = request.ContainsAnyWord("what", "whats", "tell", "current");

            if (!asksForTime && !asksForDate) return 0;
            return isQuestion ? 80 : 40;
        }
    }
}
