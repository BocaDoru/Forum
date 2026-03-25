using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages.Questions;

public class EditQuestionMessage(
    ILogger<EditQuestionMessage> logger,
    IQuestionsService questionsService
) : Message<EditQuestionRequest, Question>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question> HandleMessage(EditQuestionRequest request, CancellationToken cancellationToken)
    { 
        List<string> tagsId =  request.TagIds.ToList();
        Status status = StatusExtensions.StatusFromString(request.Status);

        return await _service.EditQuestionAsync(request.QuestionId, request.UserId, request.Title, request.Text, request.ImageId, tagsId, status);
    }
}
