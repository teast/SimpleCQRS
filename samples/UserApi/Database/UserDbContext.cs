using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using UserApi.Database.Models;

namespace UserApi.Database;

public class UserDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<UserEvent> UserEvents { get; set; } = default!;

    public UserDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
        });

        modelBuilder.Entity<UserEvent>(builder =>
        {
            builder.HasKey(e => new { e.UserId, e.Version });

            // Because we are storing our event data as a json blob we need to property convert it and storing the type name so we know what UserEvent to actual deserialize the json back to
            builder.Property(e => e.EventData)
            .HasConversion(
                @event => Newtonsoft.Json.JsonConvert.SerializeObject(@event, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                data => JsonConvert.DeserializeObject<CQRS.Events.UserEvent>(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })!
            );
        });
    }
}
