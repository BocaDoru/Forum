using commons.Protos;
using usersServiceClient;

namespace http.Endpoints.Users;

public class UpdateUsername(ILogger<UpdateUsername> logger) : ForumEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Patch("api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "Invalid username.", cancellationToken);
            return;
        }

        var grpcReq = new UsernameRequest
        {
            UserId = GetUserId(),
            Username = req,
        };

        MessageResponse response = await Client.UpdateUsernameAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

