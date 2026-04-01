using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using votesServiceClient;

namespace forum.Message.Votes;

public class CastVoteMessage(
    ILogger<CastVoteMessage> logger,
    IVotesService votesService
) : Message<CastVoteRequest, Vote>(logger)
{
    private readonly IVotesService _service = votesService;

    protected override async Task<Vote> HandleMessage(CastVoteRequest request, CancellationToken cancellationToken)
        => await _service.CastVote(request.UserId, request.PostId, request.IsQuestion, request.IsDownvote);
}