using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using commentsServiceClient;

namespace forum.Messages.Comments;

public class CreateCommentMessage(ILogger<CreateCommentMessage> logger, ICommentsService commentsService)
    : Message<CreateCommentRequest, Comment>(logger)
{
    protected override async Task<Comment> HandleMessage(CreateCommentRequest request, CancellationToken cancellationToken)
    {
        return await commentsService.CreateCommentAsync(request.QuestionId, request.AuthorId, request.Text, request.ImageId);
    }
}
