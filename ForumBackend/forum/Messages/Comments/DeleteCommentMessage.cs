using commons.MessageBase;
using forum.Implementations;
using commentsServiceClient;

namespace forum.Messages.Comments;

public class DeleteCommentMessage(ILogger<DeleteCommentMessage> logger, ICommentsService commentsService)
    : Message<DeleteCommentRequest, bool>(logger)
{
    protected override async Task<bool> HandleMessage(DeleteCommentRequest request, CancellationToken cancellationToken)
    {
        return await commentsService.DeleteCommentAsync(request.CommentId, request.AuthorId);
    }
}
