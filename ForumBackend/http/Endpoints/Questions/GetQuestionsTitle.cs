using commons.Protos;
using FastEndpoints;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class GetQuestionsByTitle(ILogger<GetQuestionsByTitle> logger) : ForumEndpoint(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/title={title}/page={page}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var title = Route<string>("title");

        if (string.IsNullOrEmpty(title))
        {
            await HandleErrorsAsync(400, "Title can not be empty.", cancellationToken);
            return;
        }

        int page = Route<int>("page");

        var grpcReq = new QuestionsByTitleRequest
        {
            Title = title,
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

