using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Comments;

public record EditCommentApiRequest(string CommentId, string Text, string? ImageId);

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
        if (string.IsNullOrWhiteSpace(userId))
        {
            await HandleErrorsAsync(401, "User is not fully authenticated.", cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.CommentId))
        {
            await HandleErrorsAsync(400, "Comment ID cannot be empty.", cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.Text))
        {
            await HandleErrorsAsync(400, "Comments need to have text.", cancellationToken);
            return;
        }

        var grpcReq = new EditCommentRequest
        {
            AuthorId = userId,
            CommentId = req.CommentId,
            Text = req.Text
        };

        if (req.ImageId is not null)
        {
            grpcReq.ImageId = req.ImageId;
        }

        var response = await Client.EditCommentAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
