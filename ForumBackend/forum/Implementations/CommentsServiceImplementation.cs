using commons;
using commons.MessageBase;
using database;
using forum.Models;
using MongoDB.Driver;
using usersServiceClient;

namespace forum.Implementations;

public interface ICommentsService
{
    Task<Comment> CreateCommentAsync(string questionId, string authorId, string text, string? imageId);
    Task<Comment> EditCommentAsync(string commentId, string authorId, string text, string? imageId);
    Task<bool> DeleteCommentAsync(string commentId, string authorId);
    Task<List<Comment>> GetCommentsForQuestionAsync(string questionId);
}

public class CommentsServiceImplementation(
    ILogger<CommentsServiceImplementation> logger,
    IDatabase database,
    usersService.usersServiceClient usersService,
    IQuestionsService questionService
) : ICommentsService
{
    private readonly ILogger<CommentsServiceImplementation> _logger = logger;
    private readonly usersService.usersServiceClient _userService = usersService;
    private readonly IQuestionsService _questionsService = questionService;
    private readonly AsyncLazy<IDatabaseCollection<Comment>> _comments = new(() => GetCommentCollection(database));

    public async Task<Comment> CreateCommentAsync(string questionId, string authorId, string text, string? imageId)
    {
  
        var userRes = await _userService.GetUserByIdAsync(new UserIdRequest { Id = authorId });
        if (userRes is null || !userRes.Success)
        {
            _logger.LogWarning("CreateComment failed: Author {authorId} not found.", authorId);
            throw new NotFoundException("User not found.");
        }

        var comments = await _comments;
        var questionExists = await _questionsService.GetQuestionByIdAsync(questionId);

        if (questionExists is null)
        {
            _logger.LogWarning("CreateComment failed: Question {questionId} not found.", questionId);
            throw new NotFoundException("Question not found.");
        }

        if (questionExists.Status == Status.SOLVED)
        {
            _logger.LogWarning("CreateComment failed: Question {questionId} is already solved.", questionId);
            throw new BadRequestException("Cannot add comments to a question that is marked as SOLVED.");
        }

        if (questionExists.Status == Status.RECEIVED)
        {
            await _questionsService.EditQuestionAsync(
                questionId,
                questionExists.AuthorId,
                questionExists.Title,
                questionExists.Text,
                questionExists.ImageId,
                questionExists.TagIds,
                Status.IN_PROGRESS
            );
            _logger.LogInformation("Question {questionId} status updated to IN_PROGRESS upon receiving first comment.", questionId);
        }

        var comment = new Comment
        {
            QuestionId = questionId,
            AuthorId = authorId,
            Text = text,
            ImageId = imageId,
            CreatedAt = DateTime.UtcNow
        };

        comment.Id = await comments.InsertAsync(comment);
        _logger.LogInformation("New comment added {id} to question {questionId}", comment.Id, questionId);

        return comment;
    }

    public async Task<Comment> EditCommentAsync(string commentId, string authorId, string text, string? imageId)
    {
        var collections = await _comments;
        var comments = await collections.GetOneByIdAsync(commentId);
        if (comments is null) throw new NotFoundException("Comment not found.");

        if (comments.AuthorId != authorId)
        {
            var res = await _userService.GetUserByIdAsync(new UserIdRequest { Id = authorId });
            if (res is null || !res.Success || !res.Payload.GetBool("IsModerator"))
                throw new ForbiddenException("Only the author or a moderator can edit this comment.");
        }

        comments.Text = text;
        comments.ImageId = imageId;
        await collections.ReplaceAsync(comments);
        return comments;
    }

    public async Task<bool> DeleteCommentAsync(string commentId, string authorId)
    {
        var collections = await _comments;
        var comment = await collections.GetOneByIdAsync(commentId);
        if (comment is null) throw new NotFoundException("Comment not found.");

        if (comment.AuthorId != authorId)
        {
            var res = await _userService.GetUserByIdAsync(new UserIdRequest { Id = authorId });
            if (res is null || !res.Success || !res.Payload.GetBool("IsModerator"))
                throw new ForbiddenException("Only the author or a moderator can delete this comment.");
        }

        return await collections.DeleteWithIdAsync(commentId);
    }

    public async Task<List<Comment>> GetCommentsForQuestionAsync(string questionId)
    {
        var collections = await _comments;
        return await collections.MongoCollection.Aggregate()
            .Match(Builders<Comment>.Filter.Eq(c => c.QuestionId, questionId))
            .SortBy(c => c.CreatedAt)
            .ToListAsync();
    }

    internal static async Task<IDatabaseCollection<Comment>> GetCommentCollection(IDatabase database)
    {
        var collection = database.GetCollection<Comment>();
        var questionIdIndex = Builders<Comment>.IndexKeys.Ascending(c => c.QuestionId);
        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<Comment>(questionIdIndex, new CreateIndexOptions { Name = "questionIdIndex" })
        ]);
        return collection;
    }
}
