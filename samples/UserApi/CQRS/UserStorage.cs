using Microsoft.EntityFrameworkCore;
using Teast.SimpleCQRS;
using UserApi.Database;
using UserApi.Database.Models;

namespace UserApi.CQRS;

public class UserStorage(UserDbContext context) : IStorage<Database.Models.UserEvent, Events.UserEvent, Database.Models.User, int>
{
    public async Task AddEventAsync(int aggregateId, Database.Models.UserEvent record)
    {
        await context.UserEvents.AddAsync(record with {
            UserId = aggregateId,
            EventType = record.EventData.GetType().Name,
        });
    }

    public async Task<IEnumerable<Database.Models.UserEvent>> GetEventsBeforeAsync(int aggregateId, DateTimeOffset upToDate)
    {
        var events = await context.UserEvents.Where(e => e.UserId == aggregateId && e.Timestamp <= upToDate).ToListAsync();
        return events;
    }

    public async Task<IEnumerable<Database.Models.UserEvent>> GetEventsAsync(int aggregateId, User snapshot)
    {
        var events = await context.UserEvents.Where(e => e.UserId == aggregateId && e.Version > snapshot.Version).ToListAsync();
        return events;
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
