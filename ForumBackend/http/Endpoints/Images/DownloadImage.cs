using commons.Protos;
using FastEndpoints;
using http.Auth;
using imagesServiceClient;
using Microsoft.AspNetCore.StaticFiles;

namespace http.Endpoints.Images;

public class DownloadImage(ILogger<DownloadImage> logger) : ForumEndpoint(logger)
{
    public imagesService.imagesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/images/{imageId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? imageId = Route<string>("imageId");

        var grpcReq = new DownloadRequest
        {
            FileId = imageId
        };

        MessageResponse grpcResponse = await Client.DownloadAsync(grpcReq, null, null, cancellationToken);
        if (grpcResponse is null || string.IsNullOrEmpty(grpcResponse.Body))
        {
            await HandleErrorsAsync(grpcResponse?.Code ?? 500, grpcResponse?.Errors ?? "Internal Error", cancellationToken);
            return;
        }

        var payload = grpcResponse.Payload;
        string fileName = payload.GetString("FileName");
        byte[] data = payload.GetBytesFromBase64("Data");

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        await Send.BytesAsync(data, fileName, contentType, null, false, cancellationToken);
    }
}
