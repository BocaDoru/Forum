using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using questionsServiceClient;

namespace forum.Messages.Questions;

public class CreateQuestionMessage(
    ILogger<CreateQuestionMessage> logger,
    IQuestionsService questionsService
) : Message<CreateQuestionRequest, Question>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question> HandleMessage(CreateQuestionRequest request, CancellationToken cancellationToken)
    { 
        List<string> tagsId =  request.TagIds.ToList();
        return await _service.CreateQuestionAsync(request.AuthorId, request.Title, request.Text, request.ImageId, tagsId);
    }
}
