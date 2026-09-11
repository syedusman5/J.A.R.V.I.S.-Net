using Jarvis.Cli.Service;
using Jarvis.Core.Interface;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Cli
{
    public sealed class ConsoleLoop(SkillRouter router, ISpeechOutput speech, IHostApplicationLifetime lifetime, SpeechToTextService _speechToTextService, WhisperModelService modelService) : BackgroundService
    {
        private static readonly string[] ExitWords = ["exit", "quit", "bye", "goodbye"];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Downloading/checking Whisper model...");

            await modelService.DownloadModelAsync();

            string welcomeMessage = "Hello, I am Jarvis, your personal assistant. How can I help you today?";

            Console.WriteLine(welcomeMessage);
            speech.SpeakAsync(welcomeMessage);
            Console.WriteLine();

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.ReadLine();
                Console.Write("> Listening...");

                var recorder = new AudioRecorder();

                var audioFile = await recorder.RecordAsync(TimeSpan.FromSeconds(5));

                try
                {
                    var text = await _speechToTextService.TranscribeAsync(audioFile);

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        Console.WriteLine("I didn't hear anything.");
                        continue;
                    }

                    Console.WriteLine($"You: {text}");

                    var input = text.Trim();

                    if (ExitWords.Contains(input.Trim().ToLowerInvariant()))
                    {
                        Console.WriteLine("Goodbye.");
                        break;
                    }
                    var result = await router.HandleAsync(text, stoppingToken);

                    Console.WriteLine(result.DisplayText);
                    speech.SpeakAsync(result.SpeechText);
                    Console.WriteLine();

                }
                finally
                {
                    if (File.Exists(audioFile))
                    {
                        File.Delete(audioFile);
                    }
                }
            }

            lifetime.StopApplication();
        }
    }
}
