using commons.MessageBase;
using votes.Implementations;
using votes.Models;
using votesServiceClient;

namespace votes.Messages;

public class CastVoteMessage( ILogger<CastVoteMessage> logger, IVotesService votesService) : Message<CastVoteRequest, (bool, string, int)>(logger)
{
    private readonly IVotesService _service = votesService;

    protected override async Task<(bool, string, int)> HandleMessage(CastVoteRequest request, CancellationToken cancellationToken)
        => await _service.CastVote(request.UserId, request.PostId, request.Type, request.VoteValue);
}