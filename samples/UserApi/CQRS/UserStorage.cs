using Microsoft.EntityFrameworkCore;
using SimpleCQRS;
using UserApi.Database;
using UserApi.Database.Models;

namespace UserApi.CQRS;

public class UserStorage(UserDbContext context) : IStorage<Events.UserEvent, Database.Models.User, int>
{
    public async Task AddEventAsync(int aggregateId, int eventVersion, Events.UserEvent @event)
    {
        await context.UserEvents.AddAsync(new UserEvent
        {
            UserId = aggregateId,
            Version = eventVersion,
            Timestamp = DateTimeOffset.UtcNow,
            EventType = @event.GetType().Name,
            EventData = @event
        });
    }

    public async Task<IEnumerable<Events.UserEvent>> GetEventsAsync(int aggregateId, User snapshot)
    {
        var events = await context.UserEvents.Where(e => e.UserId == aggregateId && e.Version > snapshot.Version).ToListAsync();
        return events.Select(e => e.EventData);
    }

    public async Task<int> GetMaxVersionAsync(int aggregateId)
    {
        var version = await context.UserEvents
            .Where(e => e.UserId == aggregateId)
            .MaxAsync(e => (int?)e.Version) ?? 0;
        return version;
    }

    public async Task<User?> GetSnapshotAsync(int aggregateId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == aggregateId);
        return user;
    }

    public Task SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    public async Task UpdateSnapshotAsync(int aggregateId, int latestSnapshotVersion, User data)
    {
        context.Users.RemoveRange(context.Users.Where(u => u.Id == aggregateId));
        await context.Users.AddAsync(data);
    }
}
