using commons.Protos;
using FastEndpoints;
using forumServiceClient;

namespace http.Endpoints.Comments;

public class GetComments(ILogger<GetComments> logger) : ForumEndpoint(logger)
{
    public commentsService.commentsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/comments/question/{questionId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var questionId = Route<string>("questionId");

        if (string.IsNullOrEmpty(questionId))
        {
            await HandleErrorsAsync(400, "QuestionId is required.", cancellationToken);
            return;
        }

        var grpcReq = new GetCommentsRequest
        {
            QuestionId = questionId
        };

        var response = await Client.GetCommentsForQuestionAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
