using commons.Protos;
using http.Auth;
using http.Endpoints;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUserById(ILogger<GetUserById> logger) : ForumEndpoint<string>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/user={id}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        string? id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await HandleErrorsAsync(400, "User ID not found", cancellationToken);
            return;
        }

        var grpcReq = new UserIdRequest
        {
            Id = id
        };

        MessageResponse response = await Client.GetUserByIdAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
