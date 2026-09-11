using Whisper.net;

public class SpeechToTextService : IDisposable
{
    private readonly WhisperFactory _factory;
    private readonly WhisperProcessor _processor;

    public SpeechToTextService(string modelPath)
    {
        _factory = WhisperFactory.FromPath(modelPath);

        _processor = _factory
            .CreateBuilder()
            .WithLanguage("auto")
            .Build();
    }

    public async Task<string> TranscribeAsync(string audioFile, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(audioFile);

        var result = new List<string>();

        await foreach (var segment in _processor.ProcessAsync(stream, cancellationToken))
        {
            result.Add(segment.Text);
        }

        return string.Join(" ", result).Trim();
    }

    public void Dispose()
    {
        _processor.Dispose();
        _factory.Dispose();
    }
}