using Microsoft.Extensions.Logging;
using commons.Protos;

namespace commons.MessageBase;

public interface IMessageHandler<in TReq> where TReq : IRequestBase
{
    Task<MessageResponse> Handle(TReq request, CancellationToken cancellationToken);
}

public sealed class EmptyResponse
{
    public static EmptyResponse EmptyValue = new EmptyResponse();

    public override string ToString() => "Empty";
}

public abstract class Message<TReq>(
    ILogger<Message<TReq>> logger
) : MessageBase<TReq, EmptyResponse>(logger) where TReq : IRequestBase
{
    protected abstract Task HandleMessage(TReq request, CancellationToken cancellationToken);

    protected sealed override async Task<EmptyResponse> HandleInternal(TReq request, CancellationToken cancellationToken)
    {
        await HandleMessage(request, cancellationToken);
        return EmptyResponse.EmptyValue;
    }
}

public abstract class Message<TReq, TRes>(
    ILogger<Message<TReq, TRes>> logger
) : MessageBase<TReq, TRes>(logger) where TReq : IRequestBase
{
    protected abstract Task<TRes> HandleMessage(TReq request, CancellationToken cancellationToken);

    protected sealed override async Task<TRes> HandleInternal(TReq request, CancellationToken cancellationToken)
        => await HandleMessage(request, cancellationToken);
}

public abstract class MessageBase<TReq, TRes>(
    ILogger<MessageBase<TReq, TRes>> logger
) : IMessageHandler<TReq> where TReq : IRequestBase
{
    protected readonly ILogger _logger = logger;
    public async Task<MessageResponse> Handle(TReq request, CancellationToken cancellationToken)
    {
        bool success = false;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Handler"] = GetType().Name,
            ["RequestType"] = typeof(TReq).Name,
            ["ResponseType"] = typeof(TRes).Name
        }))
        {
            _logger.LogInformation("Starting message request.");

            string? validationError = request.Validate();
            if (!string.IsNullOrEmpty(validationError))
            {
                _logger.LogError("Validation failed: {error}.", validationError);
                return MessageResponse.BadRequest(validationError);
            }

            try
            {
                TRes res = await HandleInternal(request, cancellationToken);

                success = true;
                return MessageResponse.Ok(res is EmptyResponse ? null : res);
            }
            catch (ServiceMessageException ex)
            {
                _logger.LogWarning(ex, "Handled Service Exception:{exName}, Msg:{msg}, Trace:{trace}.", ex.GetType().Name, ex.Message, ex.StackTrace);
                return ex.ToResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception:{exName}, Msg:{msg}, Trace:{trace}.", ex.GetType().Name, ex.Message, ex.StackTrace);
                return MessageResponse.Error(ex);
            }
            finally
            {
                _logger.LogInformation("Ending message request Result:{res}", success ? "Success" : "Fail");
            }
        }
    }

    protected abstract Task<TRes> HandleInternal(TReq request, CancellationToken cancellationToken);
}
