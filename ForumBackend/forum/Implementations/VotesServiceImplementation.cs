using database;
using forum.Models;
using commons.MessageBase;
using MongoDB.Driver;
using usersServiceClient;

namespace forum.Implementations;

public enum VoteAction { Added, Removed, Switched }

public record VoteResult(Vote Vote, VoteAction Action, bool PreviousIsDownvote);

public interface IVotesService
{
    Task<Vote> CastVote(string userId, string postId, bool isQuestion, bool isDownvote);
    Task<int> GetVoteCount(string postId, bool isQuestion);
}

public class VotesServiceImplementation(
    ILogger<VotesServiceImplementation> logger,
    IDatabase database,
    IQuestionsService questionsService,
    usersService.usersServiceClient usersService
) : IVotesService
{
    private readonly ILogger<VotesServiceImplementation> _logger = logger;
    private readonly IQuestionsService _questionsService = questionsService;
    private readonly usersService.usersServiceClient _usersService = usersService;
    private readonly AsyncLazy<IDatabaseCollection<Vote>> _votes = new(() => GetVotesCollection(database));

    public async Task<Vote> CastVote(string userId, string postId, bool isQuestion, bool isDownvote)
    {
        string authorId = await CheckSelfVote(userId, postId, isQuestion);

        var result = await UpsertVote(userId, postId, isQuestion, isDownvote);

        int voteCountDelta = ComputeVoteCountDelta(result);
        await _questionsService.UpdateVoteCountAsync(postId, voteCountDelta);

        var (authorDelta, voterDelta) = ComputeScoreDeltas(result, isQuestion);

        if (authorDelta != 0)
        {
            await _usersService.UpdateUserScoreAsync(new UpdateUserScoreRequest
            {
                UserId = authorId,
                Delta = authorDelta
            });
            _logger.LogInformation("Author:{aId} score updated by delta:{d}", authorId, authorDelta);
        }

        if (voterDelta != 0)
        {
            await _usersService.UpdateUserScoreAsync(new UpdateUserScoreRequest
            {
                UserId = userId,
                Delta = voterDelta
            });
            _logger.LogInformation("Voter:{vId} score updated by delta:{d}", userId, voterDelta);
        }

        return result.Vote;
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

    // Returns authorId so CastVote can use it for score update
    private async Task<string> CheckSelfVote(string userId, string postId, bool isQuestion)
    {
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
                _logger.LogInformation("User:{uId} tried to vote on their own question:{pId}.", userId, postId);
                throw new BadRequestException("You cannot vote on your own post.");
            }
            return question.AuthorId;
        }
        // future: comments
        // else
        // {
        //     var comment = await _commentsService.GetCommentByIdAsync(postId);
        //     if (comment is null) throw new NotFoundException("Comment not found.");
        //     if (comment.AuthorId == userId) throw new BadRequestException("You cannot vote on your own post.");
        //     return comment.AuthorId;
        // }

        return string.Empty;
    }

    private async Task<VoteResult> UpsertVote(string userId, string postId, bool isQuestion, bool isDownvote)
    {
        var votes = await _votes;

        var existingVote = await votes.GetOneAsync(v =>
            v.UserId == userId && v.PostId == postId && v.IsQuestion == isQuestion);

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
            return new VoteResult(newVote, VoteAction.Added, isDownvote);
        }
        else if (existingVote.IsDownvote == isDownvote)
        {

            await votes.DeleteWithIdAsync(existingVote.Id);
            _logger.LogInformation("User:{uId} removed their vote on post:{pId}", userId, postId);
            return new VoteResult(existingVote, VoteAction.Removed, existingVote.IsDownvote);
        }
        else
        {
            // Different vote -> update
            await votes.UpdateByIdAsync(existingVote.Id, v => v.IsDownvote, isDownvote);
            _logger.LogInformation("User:{uId} switched vote on post:{pId} (isDownvote:{d})", userId, postId, isDownvote);
            return new VoteResult(existingVote with { IsDownvote = isDownvote }, VoteAction.Switched, existingVote.IsDownvote);
        }
    }

    private int ComputeVoteCountDelta(VoteResult result) => result.Action switch
    {
        VoteAction.Added => result.Vote.IsDownvote ? -1 : 1,
        VoteAction.Removed => result.Vote.IsDownvote ? 1 : -1,
        VoteAction.Switched => result.Vote.IsDownvote ? -2 : 2,
        _ => 0
    };

    private (float authorDelta, float voterDelta) ComputeScoreDeltas(VoteResult result, bool isQuestion)
    {
        float authorDelta = (result.Action, result.Vote.IsDownvote, isQuestion) switch
        {
            // Added
            (VoteAction.Added, false, true) => 2.5f,
            (VoteAction.Added, false, false) => 5.0f,
            (VoteAction.Added, true, true) => -1.5f,
            (VoteAction.Added, true, false) => -2.5f,
            // Removed (reverse)
            (VoteAction.Removed, false, true) => -2.5f,
            (VoteAction.Removed, false, false) => -5.0f,
            (VoteAction.Removed, true, true) => 1.5f,
            (VoteAction.Removed, true, false) => 2.5f,
            // Switched down -> up
            (VoteAction.Switched, false, true) => 4.0f,
            (VoteAction.Switched, false, false) => 7.5f,
            // Switched up -> down
            (VoteAction.Switched, true, true) => -4.0f,
            (VoteAction.Switched, true, false) => -7.5f,
            _ => 0f
        };

        float voterDelta = (result.Action, result.Vote.IsDownvote) switch
        {
            (VoteAction.Added, true) => -1.5f,
            (VoteAction.Removed, true) => 1.5f,
            (VoteAction.Switched, true) => -1.5f,
            (VoteAction.Switched, false) => 1.5f,
            _ => 0f
        };

        return (authorDelta, voterDelta);
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