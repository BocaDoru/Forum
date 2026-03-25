using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Users;

public class CreateTag(ILogger<CreateTag> logger) : ForumEndpoint<string>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/questions/tag");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Question tag need to have a name.", cancellationToken);
            return;
        }

        var grpcReq = new CreateTagRequest
        {
            Name = req
        };

        MessageResponse response = await Client.CreateTagAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

