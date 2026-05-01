using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class ChangePasswordMessage(
    ILogger<ChangePasswordMessage> logger,
    IUsersService userService
) : Message<ChangePasswordRequest>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task HandleMessage(ChangePasswordRequest request, CancellationToken cancellationToken)
        => await _service.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
}