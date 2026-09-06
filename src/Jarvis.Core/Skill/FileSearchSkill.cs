using Jarvis.Core.Interface;
using Jarvis.Core.Service;
using Microsoft.Extensions.Options;

namespace Jarvis.Core.Skill
{

    public sealed class FileSearchOptions
    {
        public const string SectionName = "FileSearch";
        public string SandboxRoot { get; set; } = @"D:\";

        public int MaxResults { get; set; } = 10;
    }
    public sealed class FileSearchSkill(IOptions<FileSearchOptions> options) : ISkill
    {
        public string Name => "files";
        public string Description => "Finds files by name inside your sandbox folder.";
        public IReadOnlyList<string> Examples => ["find file budget", "search for invoice"];

        public int Score(UserRequest request)
        {
            var verb = request.ContainsAnyWord("find", "search", "locate", "look");
            var noun = request.ContainsAnyWord("file", "files", "document", "documents", "folder");

            if (verb && noun) return 90;
            if (request.Normalized.StartsWith("find ")) return 45;
            return 0;
        }

        public Task<SkillResult> ExecuteAsync(UserRequest request, CancellationToken ct)
        {
            var term = ExtractSearchTerm(request);
            if (string.IsNullOrWhiteSpace(term))
                return Task.FromResult(SkillResult.Fail("Tell me what to search for, e.g. \"find file budget\"."));

            var root = Path.GetFullPath(options.Value.SandboxRoot);
            if (!Directory.Exists(root))
                return Task.FromResult(SkillResult.Fail($"Sandbox folder doesn't exist: {root}"));

            var enumeration = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,      // skip permission-denied dirs instead of throwing
                MatchCasing = MatchCasing.CaseInsensitive,
                MaxRecursionDepth = 8
            };

            var matches = new List<string>();

            foreach (var path in Directory.EnumerateFiles(root, $"*{term}*", enumeration))
            {
                ct.ThrowIfCancellationRequested();

                // Defence in depth: symlinks can point outside the sandbox.
                var full = Path.GetFullPath(path);
                if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;

                matches.Add(Path.GetRelativePath(root, full));
                if (matches.Count >= options.Value.MaxResults) break;
            }

            if (matches.Count == 0)
                return Task.FromResult(SkillResult.Ok($"No files matching \"{term}\" under {root}."));

            var display = $"Found {matches.Count} file(s) under {root}:\n  " + string.Join("\n  ", matches);
            var spoken = $"I found {matches.Count} matching file{(matches.Count == 1 ? "" : "s")}.";

            return Task.FromResult(SkillResult.Ok(display, spoken));
        }

        private static string? ExtractSearchTerm(UserRequest request)
        {
            string[] noise = ["find", "search", "locate", "look", "for", "a", "the",
                          "file", "files", "document", "documents", "folder", "named", "called"];

            var words = request.Tokens.Where(t => !noise.Contains(t)).ToArray();
            return words.Length == 0 ? null : string.Join(' ', words);
        }
    }
}
