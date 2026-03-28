using commons.MessageBase;
using commons.Protos;
using Grpc.Core;
using imagesServiceClient;

namespace images;

public class ImagesService(MessageDispatcher dispatcher) : imagesService.imagesServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> Upload(UploadRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> Download(DownloadRequest request, ServerCallContext serverCallContext)
        => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);
}
