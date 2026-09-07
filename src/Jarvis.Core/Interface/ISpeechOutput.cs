
namespace Jarvis.Core.Interface
{
    public interface ISpeechOutput
    {
        Task SpeakAsync(string text, CancellationToken ct = default);
    }
    public sealed class NullSpeechOutput : ISpeechOutput
    {
        public Task SpeakAsync(string text, CancellationToken ct = default) => Task.CompletedTask;
    }
}
