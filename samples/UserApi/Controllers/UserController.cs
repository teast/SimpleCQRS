using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.CQRS;
using UserApi.CQRS.Commands;
using UserApi.Database;
using UserApi.Database.Models;

namespace UserApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(CommandHandler handler, UserDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserViewModel>>> Get()
    {
        var users = await context.Users.ToListAsync();
        var events = await context.UserEvents.ToListAsync();
        var model = users.GroupJoin(events,
                user => user.Id,
                e => e.UserId,
                UserViewModel.From).ToList();
        return Ok(model);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        var events = await context.UserEvents.Where(e => e.UserId == id).OrderBy(e => e.Version).ToListAsync();
        return Ok(UserViewModel.From(user, events));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCommand command)
    {
        await handler.Handle(command);
        return NoContent();
    }

    [HttpPut("{id}/name")]
    public async Task<IActionResult> UpdateName(int id, ChangeNameCommand command)
    {
        try
        {
            await handler.Handle(command);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id}/email")]
    public async Task<IActionResult> UpdateEmail(int id, ChangeEmailCommand command)
    {
        try
        {
            await handler.Handle(command);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id}/age")]
    public async Task<IActionResult> UpdateAge(int id, ChangeAgeCommand command)
    {
        try
        {
            await handler.Handle(command);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public class UserViewModel
{
    public UserViewModel(User user, IEnumerable<UserEventViewModel> events)
    {
        User = user;
        Events = events;
    }

    public User User { get; }
    public IEnumerable<UserEventViewModel> Events { get; }

    internal static UserViewModel From(User user, IEnumerable<UserEvent> events)
    {
        return new UserViewModel(user, events.OrderBy(e => e.Version).Select(UserEventViewModel.From).ToList());
    }
}

public class UserEventViewModel
{
    public UserEventViewModel(int version, DateTimeOffset timestamp, string eventType, Dictionary<string, object?> eventData)
    {
        Version = version;
        Timestamp = timestamp;
        EventType = eventType;
        EventData = eventData;
    }

    public int Version { get; }
    public DateTimeOffset Timestamp { get; }
    public string EventType { get; }
    public Dictionary<string, object?> EventData { get; }

    internal static UserEventViewModel From(UserEvent e)
    {
        return new UserEventViewModel(e.Version, e.Timestamp, e.EventType, GetObject(e.EventData));
    }

    private static Dictionary<string, object?> GetObject(object o)
    {
        var result = new Dictionary<string, object?>();
        var t = o.GetType();
        var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            result.Add(property.Name, property.GetValue(o));
        }

        return result;
    }
}
