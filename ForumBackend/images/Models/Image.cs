using database;

namespace images.Models;

[CollectionName("Images")]
public record Image : DatabaseModel
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath {  get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public record ImageResponse(string FileName, byte[] Data);
