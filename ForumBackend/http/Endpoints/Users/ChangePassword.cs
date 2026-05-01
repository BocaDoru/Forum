using commons.Protos;
using usersServiceClient;

namespace http.Endpoints.Users;

public record PasswordApiRequest(string OldPassword, string NewPassword);

public class ChangePassword(ILogger<ChangePassword> logger) : ForumEndpoint<PasswordApiRequest>(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Patch("api/users/password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PasswordApiRequest req, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(req.OldPassword) || string.IsNullOrEmpty(req.NewPassword))
        {
            await HandleErrorsAsync(400, "Old password and new password can not be missing.", cancellationToken);
            return;
        }

        var grpcReq = new ChangePasswordRequest
        {
            UserId = GetUserId(),
            OldPassword = req.OldPassword,
            NewPassword = req.NewPassword
        };

        MessageResponse response = await Client.ChangePasswordAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

