using database;

namespace forum.Models;

[CollectionName("Votes")]
public record Vote : DatabaseModel
{
    public required string UserId { get; set; } = string.Empty;
    public required string PostId { get; set; } = string.Empty;
    public required bool IsQuestion { get; set; } 
    public required bool IsDownvote { get; set; }
}