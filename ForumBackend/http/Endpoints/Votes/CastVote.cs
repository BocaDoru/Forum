using commons.Protos;
using votesServiceClient;
using http.Auth;

namespace http.Endpoints.Votes;

public record CastVoteApiRequest(string PostId, bool IsQuestion, bool IsDownvote);

public class CastVote(ILogger<CastVote> logger) : ForumEndpoint<CastVoteApiRequest>(logger)
{
    public votesService.votesServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/votes/cast");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(CastVoteApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.PostId))
        {
            await HandleErrorsAsync(400, "Invalid post selected.", cancellationToken);
            return;
        }

        var grpcReq = new CastVoteRequest
        {
            UserId = GetUserId(),
            PostId = req.PostId,
            IsQuestion = req.IsQuestion,
            IsDownvote = req.IsDownvote
        };

        MessageResponse response = await Client.CastVoteAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}