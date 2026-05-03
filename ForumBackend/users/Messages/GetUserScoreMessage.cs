using commons.MessageBase;
using users.Implementations;
using usersServiceClient;

namespace users.Messages;

public class GetUserScoreMessage(
    ILogger<GetUserScoreMessage> logger,
    IUsersService usersService
) : Message<GetUserScoreRequest, float>(logger)
{
    private readonly IUsersService _service = usersService;

    protected override async Task<float> HandleMessage(GetUserScoreRequest request, CancellationToken cancellationToken)
        => await _service.GetUserScore(request.Id);
}