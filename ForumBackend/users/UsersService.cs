using commons.MessageBase;
using commons.Protos;
using Grpc.Core;
using usersServiceClient;

namespace users;

public class UsersService(MessageDispatcher dispatcher) : usersService.usersServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> GetUserById(UserIdRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetUserByEmail(UserEmailRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> RegisterUser(RegisterRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> AuthUser(AuthRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> BanUser(BanRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> UnbanUser(UnbanRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> UpdateUserScore(UpdateUserScoreRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetUserScore(UserIdRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> UpdateUsername(UsernameRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> DeleteUser(UserIdRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> ChangePassword(ChangePasswordRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetAllUsers(EmptyUsersRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);
}
