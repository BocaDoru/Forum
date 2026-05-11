using commons.Protos;
using FastEndpoints;
using http.Auth;
using http.Endpoints;
using usersServiceClient;

public class GetAllUsers(ILogger<GetAllUsers> logger) : ForumEndpoint(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Get("api/users/all");
        Policies(ForumPolicy.Moderator);
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        MessageResponse response = await Client.GetAllUsersAsync(new EmptyUsersRequest(), null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}