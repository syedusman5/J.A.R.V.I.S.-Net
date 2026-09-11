using NAudio.Wave;

namespace Jarvis.Cli.Service
{
    public class AudioRecorder
    {
        public async Task<string> RecordAsync(TimeSpan duration,CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(Path.GetTempPath(),$"voice_{Guid.NewGuid():N}.wav");

            await using var recorder = new WasapiRecorderBuilder().WithFormat(new WaveFormat(16000, 16, 1)).Build();

            using var writer = new WaveFileWriter(filePath, recorder.WaveFormat);

            recorder.DataAvailable += (buffer, flags, devicePosition, qpcPosition) =>
            {
                writer.Write(buffer);
            };

            Console.WriteLine("Listening...");

            recorder.StartRecording();

            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            finally
            {
                recorder.StopRecording();
            }

            return filePath;
        }
    }
}
