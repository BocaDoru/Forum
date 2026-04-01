using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using commentsServiceClient;

namespace forum.Messages.Comments;

public class GetCommentsMessage(ILogger<GetCommentsMessage> logger, ICommentsService commentsService)
    : Message<GetCommentsRequest, List<Comment>>(logger)
{
    protected override async Task<List<Comment>> HandleMessage(GetCommentsRequest request, CancellationToken cancellationToken)
    {
        return await commentsService.GetCommentsForQuestionAsync(request.QuestionId);
    }
}
