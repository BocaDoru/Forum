using commons.MessageBase;
using images.Implementation;
using images.Models;
using imagesServiceClient;

namespace users.Messages;

public class DownloadMessage(
    ILogger<DownloadMessage> logger,
    ImageServiceImplementation userService
) : Message<DownloadRequest, ImageResponse>(logger)
{
    private readonly ImageServiceImplementation _service = userService;

    protected override async Task<ImageResponse> HandleMessage(DownloadRequest request, CancellationToken cancellationToken)
        => await _service.GetImageByIdAsync(request.FileId);
}
