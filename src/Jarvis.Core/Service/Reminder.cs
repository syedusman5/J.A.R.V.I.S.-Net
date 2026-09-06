namespace Jarvis.Core.Service
{
    public sealed record Reminder(Guid Id, string Text, DateTimeOffset DueAt);

}
