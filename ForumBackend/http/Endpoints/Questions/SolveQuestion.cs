using commons.Protos;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class SolveQuestion(ILogger<SolveQuestion> logger) : ForumEndpoint<string>(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/questions/solve");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "No question selected.", cancellationToken);
            return;
        }

        var grpcReq = new SolveRequest
        {
            QuestionId = req,
            AuthorId = GetUserId()
        };

        MessageResponse response = await Client.SolveQuestionAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

