using database;

namespace users.Models;

[CollectionName("Users")]
public record User : DatabaseModel
{
    public required string Username { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required float Score { get; set; } = float.MinValue;
    public required bool IsModerator { get; set; } = false;

    public required bool IsBanned { get; set; } = false;
}

public record UserWithJwt(User? User, string JwtToken);