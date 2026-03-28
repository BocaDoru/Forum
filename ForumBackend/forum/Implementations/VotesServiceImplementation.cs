using database;
using forum.Models;
using commons.MessageBase;
using MongoDB.Driver;

namespace forum.Implementations;

public interface IVotesService
{
    Task<Vote> CastVote(string userId, string postId, bool isQuestion, bool isDownvote);
    Task<int> GetVoteCount(string postId, bool isQuestion);
}

public class VotesServiceImplementation(
    ILogger<VotesServiceImplementation> logger,
    IDatabase database,
    IQuestionsService questionsService
) : IVotesService // need to add ICommentService commentsService
{
    private readonly ILogger<VotesServiceImplementation> _logger = logger;
    private readonly IQuestionsService _questionsService = questionsService;
    // private readonly ICommentsService _commentsService = commentsService;
    private readonly AsyncLazy<IDatabaseCollection<Vote>> _votes = new(() => GetVotesCollection(database));

    public async Task<Vote> CastVote(string userId, string postId, bool isQuestion, bool isDownvote)
    {
        var votes = await _votes;

        // Check author to prevent self-voting
        if (isQuestion)
        {
            var question = await _questionsService.GetQuestionByIdAsync(postId);
            
            if (question is null)
            {
                _logger.LogInformation("Question:{pId} was not found.", postId);
                throw new NotFoundException("Question not found.");
            }

            if (question.AuthorId == userId)
            {
                _logger.LogInformation("User:{uId} tried to vote on their own question: {pId}.", userId, postId);
                throw new BadRequestException("You cannot vote on  your own post.");
            }
        }
        else
        {
            /*
             * var comment = await _commentService.GetCommentByIdAsync(postId);
             * 
             * if (comment is null)
             * {
             * _logger.LogInformation("Comment:{pId} was not found.", postId);
             * throw new NotFoundException("Comment not found.");
             * 
             * if (comment.AuthorId == userId)
             * {
             *  _logger.LogInformation("User:{uId} tried to vote on their own question: {pId}.", userId, postId);
             *  throw new BadRequestException("You cannot vote on your own comment.");
             */
        }

        var existingVote = await votes.GetOneAsync(v => v.UserId == userId && v.PostId == postId && v.IsQuestion == isQuestion);

        if (existingVote is null)
        {
            // New vote -> Insert
            Vote newVote = new Vote
            {
                UserId = userId,
                PostId = postId,
                IsQuestion = isQuestion,
                IsDownvote = isDownvote
            };

            newVote.Id = await votes.InsertAsync(newVote);
            _logger.LogInformation("User:{uId} voted on post:{pId} (isDownvote:{d})", userId, postId, isDownvote);
            return newVote;
        }
        else if (existingVote.IsDownvote == isDownvote)
        {
            // Same vote again -> toggle off (delete)
            await votes.DeleteWithIdAsync(existingVote.Id);
            _logger.LogInformation("User:{uId} removed their vote on post:{pId}", userId, postId);
            return existingVote;
        }
        else
        {
            // Different vote -> update
            await votes.UpdateByIdAsync(existingVote.Id, v => v.IsDownvote, isDownvote);
            _logger.LogInformation("User:{uId} changed their vote on post:{pId} (isDownvote:{d})", userId, postId, isDownvote);
            return existingVote with { IsDownvote = isDownvote };
        }
    }

    public async Task<int> GetVoteCount(string postId, bool isQuestion)
    {
        var votes = await _votes;
        var allVotes = await votes.MongoCollection
            .Find(v => v.PostId == postId && v.IsQuestion == isQuestion)
            .ToListAsync();

        int upvotes = allVotes.Count(v => !v.IsDownvote);
        int downvotes = allVotes.Count(v => v.IsDownvote);

        _logger.LogInformation("Vote count for post:{pId} — upvotes:{u} downvotes:{d}", postId, upvotes, downvotes);
        return upvotes - downvotes;
    }

    internal static async Task<IDatabaseCollection<Vote>> GetVotesCollection(IDatabase database)
    {
        var collection = database.GetCollection<Vote>();

        var postIdIndex = Builders<Vote>.IndexKeys.Ascending(v => v.PostId);
        var userPostIndex = Builders<Vote>.IndexKeys
            .Ascending(v => v.UserId)
            .Ascending(v => v.PostId);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<Vote>(postIdIndex, new CreateIndexOptions { Name = "votePostIdIndex" }),
            new CreateIndexModel<Vote>(userPostIndex, new CreateIndexOptions { Name = "voteUserPostIndex", Unique = true })
        ]);

        return collection;
    }

}