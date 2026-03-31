using commons.Protos;
using FastEndpoints;
using questionsServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.UseQuestionsrs;

public record EditQuestionApiRequest(string Title, string Text, IFormFile? Image, IEnumerable<string> TagIds, string Status);
public class EditQuestion(ILogger<EditQuestion> logger) : ForumEndpoint<EditQuestionApiRequest>(logger)
{
    public questionsService.questionsServiceClient QuestionsClient { get; set; } = default!;
    public imagesService.imagesServiceClient ImagesClient { get; set; } = default!;

    public override void Configure()
    {
        Put("api/questions/{questionId}");
        AllowFileUploads();

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

        if (string.IsNullOrEmpty(req.Title) && string.IsNullOrEmpty(req.Text) && (req.TagIds is null || !req.TagIds.Any()) && string.IsNullOrEmpty(req.Status) && (req.Image is null || req.Image.Length == 0))
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

        string? imageId;
        if (req.Image is not null && req.Image.Length != 0)
        {
            using MemoryStream ms = new();
            await req.Image.CopyToAsync(ms, cancellationToken);

            byte[] bytes = ms.ToArray();

            var uploadReq = new UploadRequest
            {
                FileName = req.Image.FileName,
                Data = ByteString.CopyFrom(bytes)
            };

            MessageResponse res = await ImagesClient.UploadAsync(uploadReq, null, null, cancellationToken);
            if (!res.Success || string.IsNullOrEmpty(res.Body))
            {
                logger.LogError("Upload image failed with Code:{code}, Errors:{err}", res.Code, res.Errors);

                await HandleErrorsAsync(res.Code, res.Errors, cancellationToken);
                return;
            }

            imageId = res.Payload.GetString("Id");
            editReq.ImageId = imageId;
        }

        if (req.TagIds is not null && req.TagIds.Any())
            editReq.TagIds.AddRange(req.TagIds);

        MessageResponse response = await QuestionsClient.EditQuestionAsync(editReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

