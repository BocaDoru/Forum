using commons.MessageBase;
using commons.Protos;
using questionsServiceClient;
using Grpc.Core;

namespace forum;

public class QuestionsService(MessageDispatcher dispatcher) : questionsService.questionsServiceBase
{
    private readonly MessageDispatcher _dispatcher = dispatcher;

    public override Task<MessageResponse> CreateQuestion(CreateQuestionRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> EditQuestion(EditQuestionRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> DeleteQuestion(DeleteQuestionRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> SolveQuestion(SolveRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetQuestionById(QuestionIdRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetQuestions(QuestionsRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetQuestionsByAuthor(QuestionsByAuthorRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetQuestionsByTitle(QuestionsByTitleRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetQuestionsByTag(QuestionsByTagRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> CreateTag(CreateTagRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

    public override Task<MessageResponse> GetTagByName(TagNameRequest request, ServerCallContext serverCallContext)
       => _dispatcher.DispatchAsync(request, serverCallContext.CancellationToken);

}
