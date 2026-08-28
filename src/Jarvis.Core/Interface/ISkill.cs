using Jarvis.Core.Service;

namespace Jarvis.Core.Interface
{
    public interface ISkill
    {
        string Name { get; }
        string Description { get; }
        IReadOnlyList<string> Examples { get; }

        int Score(UserRequest request);  

        Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct);
    }
}
