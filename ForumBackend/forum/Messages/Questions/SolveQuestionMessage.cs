using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class SolveQuestionMessage(
    ILogger<SolveQuestionMessage> logger,
    IQuestionsService questionsService
) : Message<SolveRequest, Question>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question> HandleMessage(SolveRequest request, CancellationToken cancellationToken)
        => await _service.UpdateStatusAsync(request.QuestionId, Status.SOLVED, request.AuthorId);
}
