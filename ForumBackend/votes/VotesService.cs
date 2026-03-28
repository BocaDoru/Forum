using commons.MessageBase;
using commons.Protos;
using Grpc.Core;
using votesServiceClient;

namespace votes;

public class VotesGrpcService(MessageDispatcher dispatcher) : VotesService.VotesServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> CastVote(CastVoteRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetVoteCount(GetVoteCountRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

}