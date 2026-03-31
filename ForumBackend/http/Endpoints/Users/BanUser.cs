using commons.Protos;
using http.Auth;
using usersServiceClient;

namespace http.Endpoints.Users;

public record BanApiRequest(string UserId, string? Reason);
public class BanUser(ILogger<BanUser> logger) : ForumEndpoint<BanApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/ban");
        Policies(ForumPolicy.Moderator);
    }

    public override async Task HandleAsync(BanApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.UserId))
        {
            await HandleErrorsAsync(400, "No user selected for ban.", cancellationToken);
            return;
        }

        var grpcReq = new BanRequest
        {
            ModeratorId = GetUserId(),
            UserId = req.UserId
        };

        if (req.Reason is not null)
        {
            grpcReq.Reason = req.Reason;
        }

        MessageResponse response = await Client.BanUserAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

