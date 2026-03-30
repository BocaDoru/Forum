using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;
using http.Endpoints.Questions;
using imagesServiceClient;

namespace http.Endpoints;

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

        MessageResponse response = await Client.DownloadAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
