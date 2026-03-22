using commons.Protos;
using usersServiceClient;

namespace http.Endpoints.Users;

public record RegisterApiRequest(string Email, string Username, string Password, bool IsModerator);
public class Register(ILogger<Register> logger) : ForumEndpoint<RegisterApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Post("api/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            await HandleErrorsAsync(400, "Incomplete register details.", cancellationToken);
            return;
        }

        var grpcReq = new RegisterRequest
        {
            Email = req.Email,
            Username = req.Username,
            Password = req.Password,
            IsModerator = req.IsModerator
        };

        MessageResponse response = await Client.RegisterUserAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

