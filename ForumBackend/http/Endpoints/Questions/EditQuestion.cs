using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Users;

public record EditQuestionApiRequest(string Title, string Text, string? ImageId, IEnumerable<string> TagIds, string Status);
public class EditQuestion(ILogger<EditQuestion> logger) : ForumEndpoint<EditQuestionApiRequest>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Put("api/questions/{questionId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EditQuestionApiRequest req, CancellationToken cancellationToken)
    {
        string? questionId = Route<string>("questionId");
        if (string.IsNullOrEmpty(questionId))
        {
            await HandleErrorsAsync(400, "Invalid question selected.", cancellationToken);
            return;
        }

        if (string.IsNullOrEmpty(req.Title) || string.IsNullOrEmpty(req.Text) || req.TagIds is null || !req.TagIds.Any() || string.IsNullOrEmpty(req.Status))
        {
            await HandleErrorsAsync(400, "Questions need to have a title, text and one or more tags.", cancellationToken);
            return;
        }

        var grpcReq = new EditQuestionRequest
        {
            QuestionId = questionId,
            UserId = GetUserId(),
            Text = req.Text,
            Title = req.Title,
            Status = req.Status
        };

        if (req.ImageId is not null)
            grpcReq.ImageId = req.ImageId;

        grpcReq.TagIds.AddRange(req.TagIds);

        MessageResponse response = await Client.EditQuestionAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

