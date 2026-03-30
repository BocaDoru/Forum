using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class BanUserMessage(
    ILogger<BanUserMessage> logger,
    IUsersService userService
) : Message<BanRequest>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task HandleMessage(BanRequest request, CancellationToken cancellationToken)
        => await _service.BanUserAsync(request.ModeratorId, request.UserId, request.Reason);
}