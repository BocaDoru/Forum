using commons.MessageBase;
using images.Implementation;
using images.Models;
using imagesServiceClient;

namespace users.Messages;

public class UploadMessage(
    ILogger<UploadMessage> logger,
    ImageServiceImplementation userService
) : Message<UploadRequest, Image>(logger)
{
    private readonly ImageServiceImplementation _service = userService;

    protected override async Task<Image> HandleMessage(UploadRequest request, CancellationToken cancellationToken)
        => await _service.UploadImageAsync(request.FileName, [.. request.Data]);
}
