using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Questions;

public class GetQuestionById(ILogger<GetQuestionById> logger) : ForumEndpoint(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/{questionId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? questionId = Route<string>("questionId");

        var grpcReq = new QuestionIdRequest
        {
            QuestionId = questionId
        };

        MessageResponse response = await Client.GetQuestionByIdAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

