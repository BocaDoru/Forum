using database;
namespace forum.Models;

[CollectionName ("Comments")]
public record Comment : DatabaseModel
{
    public required string QuestionId { get; set; } = string.Empty;
    public required string AuthorId { get; set; } = string.Empty;
    public required string Text { get; set; } = string.Empty;
    public string? ImageId { get; set; } = null;
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
