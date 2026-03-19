using database;

namespace users.Models;

[CollectionName("Users")]
public record User : DatabaseModel
{
    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required float Score { get; set; } = float.MinValue;
}
