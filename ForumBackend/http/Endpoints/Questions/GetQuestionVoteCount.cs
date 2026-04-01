using commons.Protos;
using FastEndpoints;
using questionsServiceClient;
using http.Auth;
using http.Endpoints;

public class GetQuestionVoteCount(ILogger<GetQuestionVoteCount> logger) : ForumEndpoint(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/{questionId}/votecount");
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

        var grpcReq = new QuestionIdRequest { QuestionId = questionId };
        MessageResponse response = await Client.GetQuestionVoteCountAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}