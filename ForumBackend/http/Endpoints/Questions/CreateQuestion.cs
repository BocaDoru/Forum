using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Users;

public record CreateQuestionApiRequest(string Title, string Text, string? ImageId, IEnumerable<string> TagIds);
public class CreateQuestion(ILogger<CreateQuestion> logger) : ForumEndpoint<CreateQuestionApiRequest>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/questions/create");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(CreateQuestionApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Title) || string.IsNullOrEmpty(req.Text) || req.TagIds is null || !req.TagIds.Any())
        {
            await HandleErrorsAsync(400, "Questions need to have a title, text and one or more tags.", cancellationToken);
            return;
        }

        var grpcReq = new CreateQuestionRequest
        {
            AuthorId = GetUserId(),
            Title = req.Title,
            Text = req.Text
        };

        if (req.ImageId is not null)
            grpcReq.ImageId = req.ImageId;

        grpcReq.TagIds.AddRange(req.TagIds);

        MessageResponse response = await Client.CreateQuestionAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

