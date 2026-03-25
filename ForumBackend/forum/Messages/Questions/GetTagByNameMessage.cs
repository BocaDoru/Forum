using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages.Questions;

public class GetTagByNameMessage(
    ILogger<GetTagByNameMessage> logger,
    IQuestionsService questionsService
) : Message<TagNameRequest, QuestionTag?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<QuestionTag?> HandleMessage(TagNameRequest request, CancellationToken cancellationToken)
        => await _service.GetTagByNameAsync(request.Name);
}
