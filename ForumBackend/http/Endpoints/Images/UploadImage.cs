using commons.Protos;
using FastEndpoints;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.Images;

public class UploadImage(ILogger<UploadImage> logger) : ForumEndpoint<IFormFile>(logger)
{
    public imagesService.imagesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/images/upload");
        AllowFileUploads();

        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(IFormFile req, CancellationToken cancellationToken)
    {
        if (req is null || req.Length == 0)
        {
            await HandleErrorsAsync(400, "No file uploaded.", cancellationToken);
            return;
        }

        using MemoryStream ms = new();
        await req.CopyToAsync(ms, cancellationToken);

        byte[] bytes = ms.ToArray();

        var uploadReq = new UploadRequest
        {
            FileName = req.FileName,
            Data = ByteString.CopyFrom(bytes)
        };

        MessageResponse response = await Client.UploadAsync(uploadReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
