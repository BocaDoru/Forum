using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Comments;

public record DeleteCommentApiRequest(string CommentId);

public class DeleteComment(ILogger<DeleteComment> logger) : ForumEndpoint<DeleteCommentApiRequest>(logger)
{
    public commentsService.commentsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/comments/delete");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(DeleteCommentApiRequest req, CancellationToken cancellationToken)
    {

        var grpcReq = new DeleteCommentRequest
        {
            AuthorId = GetUserId(),
            CommentId = req.CommentId
        };

        var response = await Client.DeleteCommentAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
