using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class GetQuestionsByTitleMessage(
    ILogger<GetQuestionsByTitleMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionsByTitleRequest, List<Question>?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<List<Question>?> HandleMessage(QuestionsByTitleRequest request, CancellationToken cancellationToken)
        => await _service.FindQuestionsByTitleAsync(request.Title, request.Pagination.Skip, request.Pagination.Limit);
}
