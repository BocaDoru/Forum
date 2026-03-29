using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class GetUserByEmailMessage(
    ILogger<GetUserByEmailMessage> logger,
    IUsersService userService
) : Message<UserEmailRequest, User?>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<User?> HandleMessage(UserEmailRequest request, CancellationToken cancellationToken)
        => await _service.GetUserByEmail(request.Email);
}
