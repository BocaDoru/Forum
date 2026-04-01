using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class GetQuestionsByAuthorMessage(
    ILogger<GetQuestionsByAuthorMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionsByAuthorRequest, List<Question>?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<List<Question>?> HandleMessage(QuestionsByAuthorRequest request, CancellationToken cancellationToken)
        => await _service.GetQuestionsByAuthorAsync(request.AuthorId, request.Pagination.Skip, request.Pagination.Limit);
}