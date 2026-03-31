using commons.MessageBase;
using forum.Implementations;
using forum.Models;
using forumServiceClient;

namespace forum.Messages.Questions;

public class UpdateQuestionVoteCountMessage(
    ILogger<UpdateQuestionVoteCountMessage> logger,
    IQuestionsService questionsService
) : Message<UpdateQuestionVoteCountRequest, Question>(logger)
{
    private readonly IQuestionsService _service = questionsService;

    protected override async Task<Question> HandleMessage(UpdateQuestionVoteCountRequest request, CancellationToken cancellationToken)
        => await _service.UpdateVoteCountAsync(request.QuestionId, request.Delta);
}