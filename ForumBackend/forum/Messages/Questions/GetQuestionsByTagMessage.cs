using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages.Questions;

public class GetQuestionsByTagMessage(
    ILogger<GetQuestionsByTagMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionsByTagRequest, List<Question>?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<List<Question>?> HandleMessage(QuestionsByTagRequest request, CancellationToken cancellationToken)
        => await _service.GetQuestionsByTagAsync(request.Tag, request.Pagination.Skip, request.Pagination.Limit);
}
