using database;
using commons;
using commons.MessageBase;

namespace forum.Models;

[CollectionName("Questions")]
public record Question : DatabaseModel
{
    public required string AuthorId { get; set; } = string.Empty;
    public required string Title { get; set; } = string.Empty;
    public required string Text { get; set; } = string.Empty;
    public required Status Status { get; set; } = Status.RECEIVED;
    public string? ImageId { get; set; } = null;
    public required List<string> TagIds { get; set; } = new List<string>();
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[CollectionName("Tags")]
public record QuestionTag : DatabaseModel
{
    public required string Name { get; set; } = string.Empty;
}

public enum Status { RECEIVED, IN_PROGRESS, SOLVED };

public static class StatusExtensions
{
    public static Status StatusFromString(string status)
    {
        switch (status.Trim().ToLowerInvariant())
        {
            case "received":
                return Status.RECEIVED;
            case "in_progress":
                return Status.IN_PROGRESS;
            case "solved":
                return Status.SOLVED;
            default:
                throw new BadRequestException("Invalid value for question status.");
        }
    }

    public static string StringFromStatus(Status status)
    {
        switch (status)
        {
            case Status.RECEIVED:
                return "received";
            case Status.IN_PROGRESS:
                return "in_progress";
            case Status.SOLVED:
                return "solved";
            default:
                throw new BadRequestException("Invalid value for question status.");
        }
    }
}