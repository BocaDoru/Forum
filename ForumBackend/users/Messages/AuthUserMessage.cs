using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class AuthUserMessage(
    ILogger<AuthUserMessage> logger,
    IUsersService userService
) : Message<AuthRequest, UserWithJwt>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<UserWithJwt> HandleMessage(AuthRequest request, CancellationToken cancellationToken)
        => await _service.Auth(request.Email, request.Password);
}