using commons.Protos;
using FastEndpoints;
using http.Auth;
using http.Endpoints;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using usersServiceClient;

namespace http.Endpoints.Users;

public class GetUserByEmail(ILogger<GetUserByEmail> logger) : ForumEndpoint(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/email={email}");
        Policies(ForumPolicy.AuthenticatedUser);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        string? email = Route<string>("email");

        if (string.IsNullOrEmpty(email))
        {
            await HandleErrorsAsync(400, "User email not found", cancellationToken);
            return;
        }

        var grpcReq = new UserEmailRequest
        {
            Email = email
        };

        MessageResponse response = await Client.GetUserByEmailAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}
