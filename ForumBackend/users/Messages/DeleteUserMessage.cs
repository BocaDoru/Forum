using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class DeleteUserMessage(
    ILogger<DeleteUserMessage> logger,
    IUsersService userService
) : Message<DeleteUserRequest>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task HandleMessage(DeleteUserRequest request, CancellationToken cancellationToken)
        => await _service.DeleteUserAsync(request.Id);
}