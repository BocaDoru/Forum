using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages.Comments;

public class EditCommentMessage(ILogger<EditCommentMessage> logger, ICommentsService commentsService)
    : Message<EditCommentRequest, Comment>(logger)
{
    protected override async Task<Comment> HandleMessage(EditCommentRequest request, CancellationToken cancellationToken)
    {
        return await commentsService.EditCommentAsync(request.CommentId, request.AuthorId, request.Text, request.ImageId);
    }
}
