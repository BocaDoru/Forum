using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Comments;

public record EditCommentApiRequest(string CommentId, string Text, string? ImageId);

public class EditComment(ILogger<EditComment> logger) : ForumEndpoint<EditCommentApiRequest>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Put("api/comments/edit");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EditCommentApiRequest req, CancellationToken cancellationToken)
    {
        var grpcReq = new EditCommentRequest
        {
            AuthorId = GetUserId(),
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
