using commons.Protos;
using FastEndpoints;
using usersServiceClient;

namespace http.Endpoints.Users;

public class DeleteUser(ILogger<DeleteUser> logger) : ForumEndpoint(logger)
{
    public usersService.usersServiceClient Client { get; set; } = default!;

    public override void Configure()
    {
        Delete("api/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
    {
        var grpcReq = new DeleteUserRequest
        {
            Id = GetUserId()
        };

        MessageResponse response = await Client.DeleteUserAsync(grpcReq, null, null, cancellationToken);
        await SendAsync(response, cancellationToken: cancellationToken);
    }
}

