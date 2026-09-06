using System.Collections.Concurrent;

namespace Jarvis.Core.Service
{
    public sealed class ReminderStore
    {
        private readonly ConcurrentDictionary<Guid, Reminder> _items = new();

        public Reminder Add(string text, DateTimeOffset dueAt)
        {
            var reminder = new Reminder(Guid.NewGuid(), text, dueAt);
            _items[reminder.Id] = reminder;
            return reminder;
        }

        public IReadOnlyList<Reminder> Pending() => _items.Values.OrderBy(r => r.DueAt).ToList();

        
        public IReadOnlyList<Reminder> TakeDue(DateTimeOffset now)
        {
            var due = _items.Values.Where(r => r.DueAt <= now).ToList();

            foreach (var r in due)
                _items.TryRemove(r.Id, out _);

            return due;
        }
    }

}
