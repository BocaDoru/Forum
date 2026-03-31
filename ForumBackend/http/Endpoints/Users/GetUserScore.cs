using commons.Protos;
using FastEndpoints;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUserScore(ILogger<GetUserScore> logger) : ForumEndpoint(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/score/{userId}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? userId = Route<string>("userId");
        if (string.IsNullOrEmpty(userId))
        {
            await HandleErrorsAsync(400, "User ID is missing.", cancellationToken);
            return;
        }

        var grpcReq = new UserIdRequest { Id = userId };
        MessageResponse response = await Client.GetUserScoreAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}