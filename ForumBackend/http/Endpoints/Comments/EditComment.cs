using commons.Protos;
using commentsServiceClient;
using Google.Protobuf;
using http.Auth;
using imagesServiceClient;

namespace http.Endpoints.Comments;

public record EditCommentApiRequest(string CommentId, string? Text, string? ImageId);

public class EditComment(ILogger<EditComment> logger) : ForumEndpoint<EditCommentApiRequest>(logger)
{
    public commentsService.commentsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Put("api/comments/edit");
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

        if (string.IsNullOrWhiteSpace(req.Text) && string.IsNullOrWhiteSpace(req.ImageId))
        {
            await HandleErrorsAsync(400, "No changes were made to the comment.", cancellationToken);
            return;
        }

        var editReq = new EditCommentRequest
        {
            AuthorId = userId,
            CommentId = req.CommentId,
        };

        if (!string.IsNullOrEmpty(req.ImageId))
            editReq.ImageId = req.ImageId;

        if (!string.IsNullOrEmpty(req.Text))
            editReq.Text = req.Text;

        var response = await Client.EditCommentAsync(editReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
