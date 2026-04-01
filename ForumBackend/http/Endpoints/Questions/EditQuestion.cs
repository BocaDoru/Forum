using commons.Protos;
using FastEndpoints;
using questionsServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.UseQuestionsrs;

public record EditQuestionApiRequest(string Title, string Text, string? ImageId, IEnumerable<string> TagIds, string Status);
public class EditQuestion(ILogger<EditQuestion> logger) : ForumEndpoint<EditQuestionApiRequest>(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

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

        if (string.IsNullOrEmpty(req.Title) && string.IsNullOrEmpty(req.Text) && (req.TagIds is null || !req.TagIds.Any()) && string.IsNullOrEmpty(req.Status) && string.IsNullOrEmpty(req.ImageId))
        {
            await HandleErrorsAsync(400, "No changes were made to the question.", cancellationToken);
            return;
        }

        var editReq = new EditQuestionRequest
        {
            QuestionId = questionId,
            UserId = GetUserId(),
        };

        if (!string.IsNullOrEmpty(req.Title))
            editReq.Title = req.Title;

        if (!string.IsNullOrEmpty(req.Text))
            editReq.Text = req.Text;

        if (!string.IsNullOrEmpty(req.Status))
            editReq.Status = req.Status;

        if (!string.IsNullOrEmpty(req.ImageId))
            editReq.ImageId = req.ImageId;

        if (req.TagIds is not null && req.TagIds.Any())
            editReq.TagIds.AddRange(req.TagIds);

        MessageResponse response = await Client.EditQuestionAsync(editReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

