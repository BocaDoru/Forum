using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class UnbanUserMessage(
    ILogger<UnbanUserMessage> logger,
    IUsersService userService
) : Message<UnbanRequest>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task HandleMessage(UnbanRequest request, CancellationToken cancellationToken)
        => await _service.UnbanUserAsync(request.ModeratorId, request.UserId);
}