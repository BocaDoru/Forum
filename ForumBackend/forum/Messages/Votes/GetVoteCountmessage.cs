using commons.MessageBase;
using forum.Implementations;
using votesServiceClient;

namespace forum.Messages.Votes;

public class GetVoteCountMessage(
    ILogger<GetVoteCountMessage> logger,
    IVotesService votesService
) : Message<GetVoteCountRequest, int>(logger)
{
    private readonly IVotesService _service = votesService;

    protected override async Task<int> HandleMessage(GetVoteCountRequest request, CancellationToken cancellationToken)
        => await _service.GetVoteCount(request.PostId, request.IsQuestion);
}