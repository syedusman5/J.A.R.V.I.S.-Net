using System;
using System.Collections.Generic;
using System.Text;
using Whisper.net.Ggml;

namespace Jarvis.Cli.Service
{
    public class WhisperModelService
    {
        private readonly string _modelPath;

        public WhisperModelService(string modelPath)
        {
            _modelPath = modelPath;
        }

        public async Task DownloadModelAsync()
        {
            if (File.Exists(_modelPath))
            {
                Console.WriteLine("Whisper model already exists.");
                return;
            }

            Console.WriteLine("Downloading Whisper model...");

            await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);

            await using var file = File.Create(_modelPath);

            await modelStream.CopyToAsync(file);

            Console.WriteLine("Whisper model downloaded successfully.");
        }
    }
}
