using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class GetQuestionsMessage(
    ILogger<GetQuestionsMessage> logger,
    IQuestionsService questionsService
) : Message<QuestionsRequest, List<Question>?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<List<Question>?> HandleMessage(QuestionsRequest request, CancellationToken cancellationToken)
        => await _service.GetQuestionsAsync(request.Skip, request.Limit);
}
