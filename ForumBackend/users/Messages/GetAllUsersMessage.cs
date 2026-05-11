using commons.MessageBase;
using users.Implementations;
using users.Models;
using usersServiceClient;

public class GetAllUsersMessage(
    ILogger<GetAllUsersMessage> logger,
    IUsersService userService
) : Message<EmptyUsersRequest, List<User>>(logger)
{
    private readonly IUsersService _service = userService;

    protected override async Task<List<User>> HandleMessage(EmptyUsersRequest request, CancellationToken cancellationToken)
        => await _service.GetAllUsersAsync();
}