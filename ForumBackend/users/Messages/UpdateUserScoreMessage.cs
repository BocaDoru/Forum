using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

namespace users.Messages;

public class UpdateUserScoreMessage(
    ILogger<UpdateUserScoreMessage> logger,
    IUsersService usersService
) : Message<UpdateUserScoreRequest, User>(logger)
{
    private readonly IUsersService _service = usersService;

    protected override async Task<User> HandleMessage(UpdateUserScoreRequest request, CancellationToken cancellationToken)
        => await _service.UpdateScore(request.UserId, request.Delta);
}