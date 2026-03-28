using database;

namespace votes.Models;

[CollectionName("Votes")]

public record  Vote : DatabaseModel
{
    public required string UserId { get; set; } = string.Empty;
    public required string PostId { get; set; } = string.Empty;
    public required int Type { get; set; } // 0 = question, 1 = answer 
    public required int VoteValue { get; set; } // 1 = upvote, 0 = downvote
}