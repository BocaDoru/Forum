using commons.Protos;
using FastEndpoints;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class DeleteQuestion(ILogger<DeleteQuestion> logger) : ForumEndpoint(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/questions/{questionId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? questionId = Route<string>("questionId");
        if (string.IsNullOrEmpty(questionId))
        {
            await HandleErrorsAsync(400, "Invalid question selected.", cancellationToken);
            return;
        }

        var grpcReq = new DeleteQuestionRequest
        {
            QuestionId = questionId,
            UserId = GetUserId()
        };

        MessageResponse response = await Client.DeleteQuestionAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

