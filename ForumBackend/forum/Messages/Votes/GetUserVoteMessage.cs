using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using votesServiceClient;

public class GetUserVoteMessage(
    ILogger<GetUserVoteMessage> logger,
    IVotesService votesService
) : Message<GetUserVoteRequest, Vote?>(logger)
{
    private readonly IVotesService _service = votesService;

    protected override async Task<Vote?> HandleMessage(GetUserVoteRequest request, CancellationToken cancellationToken)
        => await _service.GetUserVote(request.UserId, request.PostId, request.IsQuestion);
}