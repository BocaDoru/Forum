using commons.Protos;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class GetQuestionsByTag(ILogger<GetQuestionsByTag> logger) : ForumEndpoint<string>(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/tag/page={page}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "No tag was selected.", cancellationToken);
            return;
        }

        int page = Route<int>("page");

        var grpcReq = new QuestionsByTagRequest
        {
            Tag = req,
            Pagination = new QuestionsRequest
            {
                Skip = (page - 1) * 20,
                Limit = 20
            },
        };

        MessageResponse response = await Client.GetQuestionsByTagAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

