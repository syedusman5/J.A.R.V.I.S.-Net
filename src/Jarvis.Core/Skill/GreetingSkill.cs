using Jarvis.Core.Interface;
using Jarvis.Core.Service;

namespace Jarvis.Core.Skill
{
    public class GreetingSkill : ISkill
    {
        public string Name => "greeting";

        public string Description => "Greets you";

        public IReadOnlyList<string> Examples => ["Hi", "Hello"];

        public Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var isGreeting = request.ContainsAnyWord("hi", "hello", "hey");

            var askingAboutMe = request.ContainsAllWords("how are you");

            var result = string.Empty;

            if (isGreeting) result = "Hi, What are we gonna build today?";

            if (askingAboutMe) result = "I'm doing good, thanks for asking.";

            return Task.FromResult(SkillResult.Ok(result));
        }

        public int Score(UserRequest request)
        {
            var isGreeting = request.ContainsAnyWord("hi", "hello", "hey");

            var askingAboutMe = request.ContainsAllWords("how are you");

            if (!isGreeting && !askingAboutMe) return 0;

            return isGreeting || askingAboutMe ? 80 : 40;  


        }
    }
}
