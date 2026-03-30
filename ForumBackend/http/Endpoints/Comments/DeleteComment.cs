using commons.Protos;
using FastEndpoints;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Comments;

public class DeleteComment(ILogger<DeleteComment> logger) : ForumEndpoint(logger)
{
    public commentsService.commentsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/comments/{commentId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? commentId = Route<string>("commentId");

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await HandleErrorsAsync(401, "User is not fully authenticated.", cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(commentId))
        {
            await HandleErrorsAsync(400, "Comment ID cannot be empty.", cancellationToken);
            return;
        }

        var grpcReq = new DeleteCommentRequest
        {
            AuthorId = userId,
            CommentId = commentId
        };

        var response = await Client.DeleteCommentAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
