using Jarvis.Cli.Service;
using Jarvis.Core.Interface;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Cli
{
    public sealed class ConsoleLoop(SkillRouter router, ISpeechOutput speech, IHostApplicationLifetime lifetime) : BackgroundService
    {
        private static readonly string[] ExitWords = ["exit", "quit", "bye", "goodbye"];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Jarvis online. Type 'help' for what I can do, 'exit' to quit.");
            Console.WriteLine();

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.Write("> ");

                var input = await Task.Run(Console.ReadLine, stoppingToken);

                if (input is null) break;                       
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (ExitWords.Contains(input.Trim().ToLowerInvariant()))
                {
                    Console.WriteLine("Goodbye.");
                    break;
                }

                var result = await router.HandleAsync(input, stoppingToken);

                Console.WriteLine(result.DisplayText);
                speech.SpeakAsync(result.SpeechText);
                Console.WriteLine();

            }

            lifetime.StopApplication();
        }
    }
}
