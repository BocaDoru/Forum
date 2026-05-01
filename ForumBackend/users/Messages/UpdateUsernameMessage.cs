using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class UpdateUsernameMessage(
    ILogger<UpdateUsernameMessage> logger,
    IUsersService userService
) : Message<UsernameRequest, User>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<User> HandleMessage(UsernameRequest request, CancellationToken cancellationToken)
        => await _service.UpdateUsernameAsync(request.UserId, request.Username);
}