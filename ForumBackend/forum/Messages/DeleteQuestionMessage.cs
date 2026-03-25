using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages;

public class DeleteQuestionMessage(
    ILogger<DeleteQuestionMessage> logger,
    IQuestionsService questionsService
) : Message<DeleteQuestionRequest, bool>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<bool> HandleMessage(DeleteQuestionRequest request, CancellationToken cancellationToken)
        => await _service.DeleteQuestionAsync(request.QuestionId, request.UserId);
}
