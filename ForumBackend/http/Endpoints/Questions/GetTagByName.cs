using commons.Protos;
using FastEndpoints;
using http.Auth;
using questionsServiceClient;

namespace http.Endpoints.Questions;

public class GetTagByName(ILogger<GetTagByName> logger) : ForumEndpoint(logger)
{
    public questionsService.questionsServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/questions/tag={tag}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? tag = Route<string>("tag");
        if (string.IsNullOrEmpty(tag))
        {
            await HandleErrorsAsync(400, "Tag can not be empty.", cancellationToken);
            return;
        }

        var grpcReq = new TagNameRequest
        {
            Name = tag
        };

        MessageResponse response = await Client.GetTagByNameAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

