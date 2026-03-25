using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages;

public class CreateTagMessage(
    ILogger<CreateTagMessage> logger,
    IQuestionsService questionsService
) : Message<CreateTagRequest, QuestionTag>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<QuestionTag> HandleMessage(CreateTagRequest request, CancellationToken cancellationToken)
        => await _service.CreateTagAsync(request.Name);
}
