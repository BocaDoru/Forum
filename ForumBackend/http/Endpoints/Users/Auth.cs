using commons.Protos;
using usersServiceClient;

namespace http.Endpoints.Users;

public record AuthApiRequest(string Email, string Password);
public class Auth(ILogger<Auth> logger) : ForumEndpoint<AuthApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/auth");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AuthApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
        {
            await HandleErrorsAsync(400, "Invalid email or password.", cancellationToken);
            return;
        }

        var grpcReq = new AuthRequest
        {
            Email = req.Email,
            Password = req.Password
        };

        MessageResponse response = await Client.AuthUserAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

