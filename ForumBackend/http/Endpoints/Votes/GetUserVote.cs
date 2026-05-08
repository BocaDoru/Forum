using commons.Protos;
using FastEndpoints;
using http.Auth;
using http.Endpoints;
using votesServiceClient;

public class GetUserVote(ILogger<GetUserVote> logger) : ForumEndpoint(logger)
{
    public votesService.votesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/votes/{postId}/{isQuestion}/mine");
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

        var grpcReq = new GetUserVoteRequest
        {
            UserId = GetUserId(),
            PostId = postId,
            IsQuestion = isQuestion
        };

        MessageResponse response = await Client.GetUserVoteAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}