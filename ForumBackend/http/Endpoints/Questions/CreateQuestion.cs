using commons.Protos;
using forumServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.Questions;

public record CreateQuestionApiRequest(string Title, string Text, IFormFile? Image, IEnumerable<string> TagIds);
public class CreateQuestion(ILogger<CreateQuestion> logger) : ForumEndpoint<CreateQuestionApiRequest>(logger)
{
    public forumService.forumServiceClient ForumClient { get; set; } = default!;
    public imagesService.imagesServiceClient ImagesClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/questions/create");
        AllowFileUploads();

        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(CreateQuestionApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Title) || string.IsNullOrEmpty(req.Text) || req.TagIds is null || !req.TagIds.Any())
        {
            await HandleErrorsAsync(400, "Questions need to have a title, text and one or more tags.", cancellationToken);
            return;
        }

        var createReq = new CreateQuestionRequest
        {
            AuthorId = GetUserId(),
            Title = req.Title,
            Text = req.Text
        };

        createReq.TagIds.AddRange(req.TagIds);

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
            createReq.ImageId = imageId;
        }

        MessageResponse response = await ForumClient.CreateQuestionAsync(createReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

