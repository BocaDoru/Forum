using commons.Protos;
using commentsServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.Comments;

public record EditCommentApiRequest(string CommentId, string? Text, IFormFile? Image);

public class EditComment(ILogger<EditComment> logger) : ForumEndpoint<EditCommentApiRequest>(logger)
{
    public commentsService.commentsServiceClient CommentsClient { get; set; } = default!;
    public imagesService.imagesServiceClient ImagesClient { get; set; } = default!;

    public override void Configure()
    {
        Put("api/comments/edit");
        AllowFileUploads();

        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EditCommentApiRequest req, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(req.CommentId))
        {
            await HandleErrorsAsync(400, "Comment ID cannot be empty.", cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.Text) && (req.Image is null || req.Image.Length == 0))
        {
            await HandleErrorsAsync(400, "No changes were made to the comment.", cancellationToken);
            return;
        }

        var editReq = new EditCommentRequest
        {
            AuthorId = userId,
            CommentId = req.CommentId,
        };

        if (!string.IsNullOrEmpty(req.Text))
            editReq.Text = req.Text;

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

        var response = await CommentsClient.EditCommentAsync(editReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
