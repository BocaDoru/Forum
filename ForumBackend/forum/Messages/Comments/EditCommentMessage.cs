using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using commentsServiceClient;

namespace forum.Messages.Comments;

public class EditCommentMessage(ILogger<EditCommentMessage> logger, ICommentsService commentsService)
    : Message<EditCommentRequest, Comment>(logger)
{
    protected override async Task<Comment> HandleMessage(EditCommentRequest request, CancellationToken cancellationToken)
    {
        string? text = request.HasText ? request.Text : null;
        string? imageId = request.HasImageId ? request.ImageId : null;

        return await commentsService.EditCommentAsync(request.CommentId, request.AuthorId, text, imageId);
    }
}
