using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class GetTagByIdMessage(
    ILogger<GetTagByIdMessage> logger,
    IQuestionsService questionsService
) : Message<TagIdRequest, QuestionTag?>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<QuestionTag?> HandleMessage(TagIdRequest request, CancellationToken cancellationToken)
        => await _service.GetTagByIdAsync(request.Id);
}
