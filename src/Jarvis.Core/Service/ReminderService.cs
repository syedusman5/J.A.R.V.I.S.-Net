using Jarvis.Core.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jarvis.Core.Service
{
    public sealed class ReminderService(
    ReminderStore store,
    TimeProvider clock,
    ILogger<ReminderService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1), clock);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    foreach (var reminder in store.TakeDue(clock.GetUtcNow()))
                    {
                        logger.LogInformation("Reminder fired: {Text}", reminder.Text);

                        Console.WriteLine();
                        Console.WriteLine($"[REMINDER] {reminder.Text}");
                        Console.Write("> ");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }
    }

}
