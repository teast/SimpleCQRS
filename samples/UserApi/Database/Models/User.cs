using Teast.SimpleCQRS;

namespace UserApi.Database.Models;

// This represents a snapshot or projection (depending on how often you want to update it)
public record User : Data<int>
{
    public User(int Id) : base(Id)
    {
    }

    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int Age { get; set; }
}
