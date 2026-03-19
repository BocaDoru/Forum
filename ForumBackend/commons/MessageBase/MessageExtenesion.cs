using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace commons.MessageBase;

public static class MessageExtenesion
{
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly();

        var handlerInterface = typeof(IMessageHandler<>);

        var handlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { Implementation = type, Interface = iface })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == handlerInterface);

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}
