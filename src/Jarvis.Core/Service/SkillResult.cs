namespace Jarvis.Core.Service
{
    public sealed record SkillResult(string DisplayText, string? SpeechText = null, bool Success = true)
    {
        public string ToSpeak => SpeechText ?? DisplayText;

        public static SkillResult Ok(string text, string? speech = null) => new(text, speech, Success: true);

        public static SkillResult Fail(string text) => new(text, SpeechText: text, Success: false);
    }
}
