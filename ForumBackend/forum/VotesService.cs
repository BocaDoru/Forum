using commons.MessageBase;
using commons.Protos;
using votesServiceClient;
using Grpc.Core;

namespace forum;

public class VotesService(MessageDispatcher dispatcher) : votesService.votesServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> CastVote(CastVoteRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetVoteCount(GetVoteCountRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetUserVote(GetUserVoteRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);
}