using commons.Protos;
using Microsoft.Extensions.DependencyInjection;

namespace commons.MessageBase;

public class MessageDispatcher(IServiceProvider serviceProvider)
{
    public Task<MessageResponse> DispatchAsync<TReq>(TReq request, CancellationToken cancellationToken) where TReq : IRequestBase
    {
        using var scope = serviceProvider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TReq>>();

        return handler.Handle(request, cancellationToken);
    }

}
