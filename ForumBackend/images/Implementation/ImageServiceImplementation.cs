using commons.MessageBase;
using database;
using DnsClient.Internal;
using images.Models;
using MongoDB.Driver;
using System.Reflection.Metadata.Ecma335;

namespace images.Implementation;

public class ImageServiceImplementation(
    ILogger<ImageServiceImplementation> logger,
    IDatabase database,
    IConfiguration config
)
{
    private readonly ILogger<ImageServiceImplementation> _logger = logger;

    private readonly AsyncLazy<IDatabaseCollection<Image>> _images = new(() => GetImageCollection(database));
    private string BasePath => config["StorageDir"] ?? "FileStorage/Images";

    public async Task<Image> UploadImageAsync(string name, byte[] data)
    {
        string extension = Path.GetExtension(name);
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
        string fileName = $"{nameWithoutExtension}_{Guid.NewGuid().ToString()}{extension}";

        var filePath = Path.Combine(BasePath, fileName);
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _logger.LogInformation("Image directory was created");
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, data);

        Image image = new Image
        {
            FileName = name,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow
        };

        var images = await _images;

        image.Id = await images.InsertAsync(image);
        _logger.LogInformation("Image:{name} was uploaded to:{path}.", fileName, filePath);

        return image;
    }

    public async Task<ImageResponse> GetImageByIdAsync(string fileId)
    {
        var files = await _images;

        Image file = await files.GetOneByIdAsync(fileId);
        if (file is null)
        {
            _logger.LogInformation($"No image was found in database with Id:{fileId}");
            throw new NotFoundException($"File with Id:{fileId} not found");
        }

        try
        {
            using var fileStream = File.OpenRead(file.FilePath);
            using MemoryStream ms = new();
            await fileStream.CopyToAsync(ms);

            return new ImageResponse(file.FileName, ms.ToArray());
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation($"No image was found in storage with Id:{fileId}");

            if (await files.DeleteWithIdAsync(fileId))
            {
                _logger.LogInformation($"Image:{fileId} deleted");
            }

            throw new NotFoundException($"Image with Id:{fileId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
            throw new InternalErrorException(ex.Message);
        }
    }

    internal static async Task<IDatabaseCollection<Image>> GetImageCollection(IDatabase database)
    {
        return database.GetCollection<Image>();
    }
}
