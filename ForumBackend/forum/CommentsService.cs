using commons.MessageBase;
using commons.Protos;
using forumServiceClient;
using Grpc.Core;

namespace forum;

public class CommentsService(MessageDispatcher dispatcher) : commentsService.commentsServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> CreateComment(CreateCommentRequest request, ServerCallContext context)
       => _dispatcher.DispatchAsync(request, context.CancellationToken);

    public override Task<MessageResponse> EditComment(EditCommentRequest request, ServerCallContext context)
       => _dispatcher.DispatchAsync(request, context.CancellationToken);

    public override Task<MessageResponse> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
       => _dispatcher.DispatchAsync(request, context.CancellationToken);

    public override Task<MessageResponse> GetCommentsForQuestion(GetCommentsRequest request, ServerCallContext context)
       => _dispatcher.DispatchAsync(request, context.CancellationToken);

}