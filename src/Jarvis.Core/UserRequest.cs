namespace Jarvis.Core
{
    public sealed class UserRequest
    {
        private static readonly char[] Separators = [' ', '\t', ',', '.', '!', '?', ';', ':', '\n', '\r'];
        public string RawText { get; }   // exactly what was said
        public string Normalized { get; }   // lower-cased, trimmed — match on this
        public string[] Tokens { get; }   // words, punctuation stripped

        public UserRequest(string rawText)
        {
            RawText = rawText ?? string.Empty;
            Normalized = RawText.Trim().ToLowerInvariant();
            Tokens = Normalized.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool ContainsAnyWord(params string[] words) => words.Any(w => Tokens.Contains(w.ToLowerInvariant()));

        public bool ContainsAllWords(string phrase)
        {
            var needed = phrase.ToLowerInvariant().Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            return needed.All(w => Tokens.Contains(w));
        }
    }
}
