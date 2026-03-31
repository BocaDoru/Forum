using commons.Protos;
using forumServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.Comments;

public record CreateCommentApiRequest(string QuestionId, string Text, IFormFile? Image);
public class CreateComment(ILogger<CreateComment> logger) : ForumEndpoint<CreateCommentApiRequest>(logger)
{
    public commentsService.commentsServiceClient Client { get; set; } = default!;

    public imagesService.imagesServiceClient ImagesClient { get; set; } = default!;

    public override void Configure()
    {
        Post("api/comments/create");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(CreateCommentApiRequest req, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(req.Text))
        {
            await HandleErrorsAsync(400, "Comments need to have text.", cancellationToken);
            return;
        }


        var grpcReq = new CreateCommentRequest
        {
            AuthorId = GetUserId(),
            QuestionId = req.QuestionId,
            Text = req.Text
        };

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
            grpcReq.ImageId = imageId;
        }

        var response = await Client.CreateCommentAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
