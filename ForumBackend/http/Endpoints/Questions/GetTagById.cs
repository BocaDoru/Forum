using commons.Protos;
using FastEndpoints;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class GetTagById(ILogger<GetTagByName> logger) : ForumEndpoint(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/tag/id={id}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await HandleErrorsAsync(400, "Tag can not be empty.", cancellationToken);
            return;
        }

        var grpcReq = new TagIdRequest
        {
            Id = id
        };

        MessageResponse response = await Client.GetTagByIdAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

