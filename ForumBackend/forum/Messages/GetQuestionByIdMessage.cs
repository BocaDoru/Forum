using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages;

public class GetQuestionByIdMessage(
    ILogger<GetQuestionByIdMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionIdRequest, Question?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question?> HandleMessage(QuestionIdRequest request, CancellationToken cancellationToken)
        => await _service.GetQuestionByIdAsync(request.QuestionId);
}
