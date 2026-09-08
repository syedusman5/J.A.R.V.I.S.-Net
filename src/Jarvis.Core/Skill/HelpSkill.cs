using Jarvis.Cli.Service;
using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using System.Text;

namespace Jarvis.Core.Skill
{
    public sealed class HelpSkill(Lazy<SkillRouter> router) : ISkill
    {
        public string Name => "help";
        public string Description => "Lists everything I can do.";
        public IReadOnlyList<string> Examples => ["help", "what can you do"];

        public int Score(UserRequest request)
        {
            if (request.Normalized is "help" or "?") return 100;
            if (request.ContainsAllWords("what can you do")) return 90;
            if (request.ContainsAnyWord("help", "commands")) return 70;
            return 0;
        }

        public Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var sb = new StringBuilder("Here's what I can do:\n");

            foreach (var skill in router.Value.Skills.OrderBy(s => s.Name))
            {
                sb.AppendLine($"  {skill.Name,-12} {skill.Description}");
                if (skill.Examples.Count > 0)
                    sb.AppendLine($"  {"",-12}   e.g. \"{skill.Examples[0]}\"");
            }

            var spokenText = "Here are the commands you can use";

            return Task.FromResult(SkillResult.Ok(sb.ToString().TrimEnd(), spokenText));
        }
    }
}
