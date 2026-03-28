using commons.Protos;
using forumServiceClient;
using http.Auth;

namespace http.Endpoints.Comments;

public record CreateCommentApiRequest(string QuestionId, string Text, string? ImageId);
public class CreateComment(ILogger<CreateComment> logger) : ForumEndpoint<CreateCommentApiRequest>(logger)
{
    public forumService.forumServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/comments/create");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(CreateCommentApiRequest req, CancellationToken cancellationToken)
    {
        var grpcReq = new CreateCommentRequest
        {
            AuthorId = GetUserId(),
            QuestionId = req.QuestionId,
            Text = req.Text
        };

        if (req.ImageId is not null)
        {
            grpcReq.ImageId = req.ImageId;
        }
        var response = await Client.CreateCommentAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
