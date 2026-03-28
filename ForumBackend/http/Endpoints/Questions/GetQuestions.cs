using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Questions;

public class GetQuestions(ILogger<GetQuestions> logger) : ForumEndpoint(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/page={page}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        int page = Route<int>("page");

        var grpcReq = new QuestionsRequest
        {
            Skip = (page - 1) * 20,
            Limit = 20
        };

        MessageResponse response = await Client.GetQuestionsAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

