using commons;
using commons.MessageBase;
using database;
using forum.Models;
using MongoDB.Driver;
using usersServiceClient;

namespace forum.Implementations;

public interface IQuestionsService
{
    Task<Question> CreateQuestionAsync(string authorId, string title, string text, string? imageId, List<string> tags);
    Task<Question> EditQuestionAsync(string questionId, string userId, string? title, string? text, string? imageId, List<string>? tags, Status? status);
    Task<bool> DeleteQuestionAsync(string questionId, string userId);
    Task<Question> UpdateStatusAsync(string questionId, Status status, string? authorId = null);
    Task<Question?> GetQuestionByIdAsync(string id);
    Task<Question> UpdateVoteCountAsync(string questionId, int delta);
    Task<List<Question>?> GetQuestionsByAuthorAsync(string authorId, int skip, int limit);
    Task<List<Question>?> FindQuestionsByTitleAsync(string title, int skip, int limit);
    Task<List<Question>?> GetQuestionsByTagAsync(string tag, int skip, int limit);
    Task<List<Question>?> GetQuestionsAsync(int skip, int limit);
    Task<QuestionTag> CreateTagAsync(string name);
    Task<QuestionTag?> GetTagByNameAsync(string name);
    Task<int> GetQuestionVoteCountAsync(string questionId);
}

public class QuestionsServiceImplementation(
    ILogger<QuestionsServiceImplementation> logger,
    IDatabase database,
    usersService.usersServiceClient usersService
) : IQuestionsService
{
    private readonly ILogger<QuestionsServiceImplementation> _logger = logger;
    private readonly usersService.usersServiceClient _userService = usersService;

    private readonly AsyncLazy<IDatabaseCollection<Question>> _questions = new(() => GetQuestionCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<QuestionTag>> _tags = new(() => GetTagCollection(database));

    public async Task<Question> CreateQuestionAsync(string authorId, string title, string text, string? imageId, List<string> tags)
    {
        var res = await _userService.GetUserByIdAsync(new UserIdRequest { Id = authorId });
        if (res is null || !res.Success)
        {
            _logger.LogError("Get user by ID filed for id:{id} with code:{code}, error:{err}", authorId, res?.Code, res?.Errors);
            throw new BadRequestException("Invalid user account.");
        }

        var questions = await _questions;

        Question newQuestion = new Question
        {
            AuthorId = authorId,
            Title = title,
            Text = text,
            TagIds = tags,
            ImageId = imageId,
            CreatedAt = DateTime.UtcNow,
            Status = Status.RECEIVED
        };

        newQuestion.Id = await questions.InsertAsync(newQuestion);

        _logger.LogInformation("New Question added(author:{auth}) {question}", authorId, newQuestion);
        return newQuestion;
    }

    public async Task<bool> DeleteQuestionAsync(string questionId, string userId)
    {
        var question = await GetQuestionByIdAsync(questionId);
        if (question is null)
        {
            _logger.LogInformation("Question:{id} could not be found.", questionId);
            throw new NotFoundException("Question not found.");
        }

        if (question.AuthorId != userId)
        {
            var res = await _userService.GetUserByIdAsync(new UserIdRequest { Id = userId });
            if (res is null || !res.Success)
            {
                _logger.LogInformation("Get user by ID filed for id:{id} with code:{code}, error:{err}.", userId, res?.Code, res?.Errors);
                _logger.LogInformation("Delete question operation will be denied for question:{qId}.", questionId);
                throw new ForbiddenException("Questions can be deleted by authors or moderators.");
            }

            var payload = res.Payload;
            bool isModerator = payload.GetBool("IsModerator");

            if (!isModerator)
            {
                _logger.LogInformation("User:{uId} does not have permission to delete this question:{qId}", userId, questionId);
                throw new ForbiddenException("Questions can be deleted by authors or moderators.");
            }

            _logger.LogInformation("Delete question was requested by moderator:{mId}.", userId);
        }

        var questions = await _questions;

        _logger.LogInformation("Question:{qId} was deleted by user:{uId}", questionId, userId);
        return await questions.DeleteWithIdAsync(questionId);
    }

    public async Task<Question> EditQuestionAsync(string questionId, string userId, string? title, string? text, string? imageId, List<string>? tags, Status? status)
    {
        var question = await GetQuestionByIdAsync(questionId);
        if (question is null)
        {
            _logger.LogInformation("Question:{id} could not be found.", questionId);
            throw new NotFoundException("Question not found.");
        }

        if (question.AuthorId != userId)
        {
            var res = await _userService.GetUserByIdAsync(new UserIdRequest { Id = userId });
            if (res is null || !res.Success)
            {
                _logger.LogInformation("Get user by ID filed for id:{id} with code:{code}, error:{err}.", userId, res?.Code, res?.Errors);
                _logger.LogInformation("Edit question operation will be denied for question:{qId}.", questionId);
                throw new ForbiddenException("Questions can be edited by authors or moderators.");
            }

            var payload = res.Payload;
            bool isModerator = payload.GetBool("IsModerator");

            if (!isModerator)
            {
                _logger.LogInformation("User:{uId} does not have permission to edit this question:{qId}", userId, questionId);
                throw new ForbiddenException("Questions can be edited by authors or moderators.");
            }

            _logger.LogInformation("Edit question was requested by moderator:{mId}.", userId);
        }

        var questions = await _questions;

        Question editedQuestion = new Question
        {
            Id = questionId,
            AuthorId = question.AuthorId,
            Title = title ?? question.Title,
            Text = text ?? question.Text,
            ImageId = imageId ?? question.ImageId,
            TagIds = tags ?? question.TagIds,
            Status = status ?? question.Status,
            CreatedAt = question.CreatedAt,
            VoteCount = question.VoteCount
        };

        await questions.ReplaceAsync(editedQuestion);

        _logger.LogInformation("Question:{qId} was edited by user:{uId}", questionId, userId);
        return editedQuestion;
    }
    
    public async Task<Question> UpdateStatusAsync(string questionId, Status status, string? authorId = null)
    {
        var question = await GetQuestionByIdAsync(questionId);
        if (question is null)
        {
            _logger.LogInformation("Question:{id} could not be found.", questionId);
            throw new NotFoundException("Question not found.");
        }

        if (status == Status.SOLVED && (authorId is null || question.AuthorId != authorId))
        {
            _logger.LogInformation("Question status can be set to SOLVED just by the author");
            throw new ForbiddenException("Status can be changed to SOLVED just by authors.");
        }

        Status oldStatus = question.Status;
        question.Status = status;

        var questions = await _questions;
        await questions.ReplaceAsync(question);

        _logger.LogInformation("Updated status:{oldStatus} to {newStatus}", oldStatus, status);
        return question;
    }

    public async Task<Question?> GetQuestionByIdAsync(string id)
    {
        var questions = await _questions;
        return await questions.GetOneByIdAsync(id);
    }

    public async Task<List<Question>?> GetQuestionsAsync(int skip, int limit) 
        => await GetQuestionsPaginatedAsync(skip, limit, FilterDefinition<Question>.Empty);

    public async Task<List<Question>?> FindQuestionsByTitleAsync(string title, int skip, int limit)
    {
        var titleFilter = Builders<Question>.Filter.Regex(q => q.Title, new MongoDB.Bson.BsonRegularExpression(title, "i"));
        return await GetQuestionsPaginatedAsync(skip, limit, titleFilter);
    }

    public async Task<List<Question>?> GetQuestionsByAuthorAsync(string authorId, int skip, int limit)
        => await GetQuestionsPaginatedAsync(skip, limit, Builders<Question>.Filter.Eq(q => q.AuthorId, authorId));

    public async Task<List<Question>?> GetQuestionsByTagAsync(string tag, int skip, int limit)
    {
        QuestionTag? qTag = await GetTagByNameAsync(tag);
        if (qTag is null)
        {
            _logger.LogInformation("Tag:{tag} was not found.", tag);
            throw new NotFoundException("This tag was not found.");
        }

        return await GetQuestionsPaginatedAsync(skip, limit, Builders<Question>.Filter.AnyEq(q => q.TagIds, qTag.Id));
    }

    public async Task<QuestionTag?> GetTagByNameAsync(string name)
    {
        var tags = await _tags;
        return await tags.GetOneAsync(t => t.Name == name);
    }

    public async Task<QuestionTag> CreateTagAsync(string name)
    {
        if (await GetTagByNameAsync(name) is not null)
        {
            _logger.LogInformation("Tag:{name} already exists.", name);
            throw new BadRequestException("This tag already exists.");
        }

        var tags = await _tags;

        QuestionTag newTag = new QuestionTag { Name = name };
        newTag.Id = await tags.InsertAsync(newTag);

        return newTag;
    }

    private async Task<List<Question>?> GetQuestionsPaginatedAsync(int skip, int limit, FilterDefinition<Question> filter)
    {
        if (skip < 0)
        {
            _logger.LogInformation("Skip can not be less than 0");
            throw new BadRequestException("Invalid skip number");
        }

        if (limit < 1 || limit > 20)
        {
            _logger.LogInformation("Limit needs to be between 1 and 20");
            throw new BadRequestException("Invalid questions number");
        }

        var questions = await _questions;
        return await questions.MongoCollection.Aggregate()
            .Match(filter)
            .SortByDescending(q => q.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    internal static async Task<IDatabaseCollection<Question>> GetQuestionCollection(IDatabase database)
    {
        var collection = database.GetCollection<Question>();

        var createdAtIndex = Builders<Question>.IndexKeys.Descending(q => q.CreatedAt);
        var authorIdIndex = Builders<Question>.IndexKeys.Ascending(q => q.AuthorId);
        var titleIndex = Builders<Question>.IndexKeys.Ascending(q => q.Title);
        var tagIdsIndex = Builders<Question>.IndexKeys.Ascending(q => q.TagIds);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<Question>(createdAtIndex, new CreateIndexOptions { Name = "createdAtIndex" }),
            new CreateIndexModel<Question>(authorIdIndex, new CreateIndexOptions { Name = "authorIdIndex" }),
            new CreateIndexModel<Question>(titleIndex, new CreateIndexOptions { Name = "titleIndex" }),
            new CreateIndexModel<Question>(tagIdsIndex, new CreateIndexOptions { Name = "tagIdsIndex" })
        ]);

        return collection;
    }

    internal static async Task<IDatabaseCollection<QuestionTag>> GetTagCollection(IDatabase database)
    {
        var collection = database.GetCollection<QuestionTag>();

        var nameIndex = Builders<QuestionTag>.IndexKeys.Ascending(t => t.Name);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<QuestionTag>(nameIndex, new CreateIndexOptions { Name = "nameIndex" , Unique = true })
        ]);

        return collection;
    }

    public async Task<Question> UpdateVoteCountAsync(string questionId, int delta)
    {
        var question = await GetQuestionByIdAsync(questionId);
        if (question is null)
        {
            _logger.LogInformation("Question:{id} could not be found.", questionId);
            throw new NotFoundException("Question not found.");
        }

        var questions = await _questions;
        await questions.UpdateByIdAsync(questionId, q => q.VoteCount, question.VoteCount + delta);

        _logger.LogInformation("Question:{qId} vote count updated by delta:{delta}", questionId, delta);
        return question with { VoteCount = question.VoteCount + delta };
    }

    public async Task<int> GetQuestionVoteCountAsync(string questionId)
    {
        var question = await GetQuestionByIdAsync(questionId);
        if (question is null)
        {
            _logger.LogInformation("Question:{id} could not be found.", questionId);
            throw new NotFoundException("Question not found.");
        }
        return question.VoteCount;
    }
}
