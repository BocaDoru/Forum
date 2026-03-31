using commons.MessageBase;
using forum.Implementations;
using forumServiceClient;

public class GetQuestionVoteCountMessage(
    ILogger<GetQuestionVoteCountMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionIdRequest, int>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<int> HandleMessage(QuestionIdRequest request, CancellationToken cancellationToken)
        => await _service.GetQuestionVoteCountAsync(request.QuestionId);
}