using commons.Protos;
using FastEndpoints;
using votesServiceClient;
using http.Auth;

namespace http.Endpoints.Votes;

public class GetVoteCount(ILogger<GetVoteCount> logger) : ForumEndpoint(logger)
{
    public votesService.votesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/votes/{postId}/{isQuestion}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? postId = Route<string>("postId");
        bool isQuestion = Route<bool>("isQuestion");

        if (string.IsNullOrEmpty(postId))
        {
            await HandleErrorsAsync(400, "Invalid post selected.", cancellationToken);
            return;
        }

        var grpcReq = new GetVoteCountRequest
        {
            PostId = postId,
            IsQuestion = isQuestion
        };

        MessageResponse response = await Client.GetVoteCountAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}