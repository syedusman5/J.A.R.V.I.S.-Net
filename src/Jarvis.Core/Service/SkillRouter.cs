using Jarvis.Core;
using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using Microsoft.Extensions.Logging;

namespace Jarvis.Cli.Service
{
    public sealed class SkillRouter(IEnumerable<ISkill> skills, ILogger<SkillRouter> logger)
    {
        private readonly IReadOnlyList<ISkill> _skills = skills.ToList();

        public IReadOnlyList<ISkill> Skills => _skills;

        public int MinimumScore = 1;

        public async Task<SkillResult> HandleAsync(string input, CancellationToken ct = default)
        {
            var request = new UserRequest(input);

            if (request.Tokens.Length > 0)
            {
                ISkill? best = null;
                var bestScore = 0;

                //Looping the skills and get the relevant skill with highest score
                foreach (var skill in _skills)
                {
                    int score;
                    try
                    {
                        score = skill.Score(request);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Skill {Skill} threw while scoring", skill.Name);
                        continue;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = skill;
                    }
                }

                if (best is not null && bestScore > MinimumScore)
                {
                    logger.LogInformation("Routing to {Skill} (score {Score})", best.Name, bestScore);

                    try
                    {
                        return await best.ExecuteAsync(request, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Skill {Skill} failed", best.Name);
                        return SkillResult.Fail($"The {best.Name} skill hit an error: {ex.Message}");
                    }
                }
                else
                {
                    logger.LogInformation("No skill matched {Input}", request.RawText);
                    return SkillResult.Fail("I don't know how to do that yet. Try 'help'.");
                }
            }
            else
            {
                return SkillResult.Ok("I didn't catch that.");
            }
        }
    }
}
