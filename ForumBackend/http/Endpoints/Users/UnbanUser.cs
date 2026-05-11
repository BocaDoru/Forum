using commons.Protos;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public class UnbanUser(ILogger<UnbanUser> logger) : ForumEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/unban");
        Policies(ForumPolicy.Moderator);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "No user selected for unban.", cancellationToken);
            return;
        }

        var grpcReq = new UnbanRequest
        {
            ModeratorId = GetUserId(),
            UserId = req
        };

        MessageResponse response = await Client.UnbanUserAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

