using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class RegisterUserMessage(
    ILogger<RegisterUserMessage> logger,
    IUsersService userService
) : Message<RegisterRequest, UserWithJwt>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<UserWithJwt> HandleMessage(RegisterRequest request, CancellationToken cancellationToken)
        => await _service.Register(request.Username, request.Email, request.Password, request.IsModerator);
}
