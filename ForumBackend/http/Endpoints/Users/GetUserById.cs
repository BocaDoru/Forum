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
        Get("api/users/user");
        AllowAnonymous();
    }

    public override async Task HandleAsync(string req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req))
        {
            await HandleErrorsAsync(400, "User ID not found", cancellationToken);
            return;
        }

        var grpcReq = new UserIdRequest
        {
            Id = req
        };

        MessageResponse response = await Client.GetUserByIdAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
