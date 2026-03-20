using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class GetUserByIdMessage(
    ILogger<GetUserByIdMessage> logger,
    IUsersService userService
) : Message<UserIdRequest, User?>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<User?> HandleMessage(UserIdRequest request, CancellationToken cancellationToken)
        => await _service.GetUserById(request.Id);
}
