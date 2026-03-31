using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class EditQuestionMessage(
    ILogger<EditQuestionMessage> logger,
    IQuestionsService questionsService
) : Message<EditQuestionRequest, Question>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question> HandleMessage(EditQuestionRequest request, CancellationToken cancellationToken)
    { 
        string? title = request.HasTitle ? request.Title : null;
        string? text = request.HasText ? request.Text : null;
        string? imageId = request.HasImageId ? request.ImageId : null;
        List<string>? tagsId =  request.TagIds.Any() ? request.TagIds.ToList() : null;
        Status? status = request.HasStatus ? StatusExtensions.StatusFromString(request.Status) : null;

        return await _service.EditQuestionAsync(request.QuestionId, request.UserId, title, text, imageId, tagsId, status);
    }
}
