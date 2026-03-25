using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Users;

public class GetQuestionsByTitle(ILogger<GetQuestionsByTitle> logger) : ForumEndpoint<string>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/title/page{page}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Title can not be empty.", cancellationToken);
            return;
        }

        int page = Route<int>("page");

        var grpcReq = new QuestionsByTitleRequest
        {
            Title = req,
            Pagination = new QuestionsRequest
            {
                Skip = (page - 1) * 20,
                Limit = 20
            },
        };

        MessageResponse response = await Client.GetQuestionsByTitleAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

