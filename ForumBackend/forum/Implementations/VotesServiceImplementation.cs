using commons.MessageBase;
using database;
using forum.Models;
using questionsServiceClient;
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
    ICommentsService commentsService,
    usersService.usersServiceClient usersService
) : IVotesService
{
    private readonly ILogger<VotesServiceImplementation> _logger = logger;
    private readonly IQuestionsService _questionsService = questionsService;
    private readonly ICommentsService _commentsService = commentsService;
    private readonly usersService.usersServiceClient _usersService = usersService;
    private readonly AsyncLazy<IDatabaseCollection<Vote>> _votes = new(() => GetVotesCollection(database));

    public async Task<Vote> CastVote(string userId, string postId, bool isQuestion, bool isDownvote)
    {
        // 1. Check self vote
        if (await IsSelfVote(userId, postId, isQuestion))
            throw new BadRequestException("You cannot vote on your own post.");

        // 2. Get existing vote
        var votes = await _votes;
        var existingVote = await votes.GetOneAsync(v =>
            v.UserId == userId && v.PostId == postId && v.IsQuestion == isQuestion);

        // 3. Insert / update / delete vote
        Vote vote;
        VoteAction action;
        bool previousIsDownvote = isDownvote;

        if (existingVote is null)
        {
            vote = new Vote
            {
                UserId = userId,
                PostId = postId,
                IsQuestion = isQuestion,
                IsDownvote = isDownvote
            };
            vote.Id = await votes.InsertAsync(vote);
            action = VoteAction.Added;
            _logger.LogInformation("User:{uId} voted on post:{pId} (isDownvote:{d})", userId, postId, isDownvote);
        }
        else if (existingVote.IsDownvote == isDownvote)
        {
            await votes.DeleteWithIdAsync(existingVote.Id);
            vote = existingVote;
            action = VoteAction.Removed;
            previousIsDownvote = existingVote.IsDownvote;
            _logger.LogInformation("User:{uId} removed vote on post:{pId}", userId, postId);
        }
        else
        {
            await votes.UpdateByIdAsync(existingVote.Id, v => v.IsDownvote, isDownvote);
            vote = existingVote with { IsDownvote = isDownvote };
            action = VoteAction.Switched;
            previousIsDownvote = existingVote.IsDownvote;
            _logger.LogInformation("User:{uId} switched vote on post:{pId} (isDownvote:{d})", userId, postId, isDownvote);
        }

        var result = new VoteResult(vote, action, previousIsDownvote);

        // 4. Compute points
        float authorDelta = ComputeAuthorDelta(result, isQuestion);
        float voterDelta = ComputeVoterDelta(result);

        // 5. Update vote count
        if (isQuestion)
            await _questionsService.UpdateVoteCountAsync(postId, ComputeVoteCountDelta(result));
        else
            await _commentsService.UpdateVoteCountAsync(postId, ComputeVoteCountDelta(result));

        // 6. Update author score
        if (authorDelta != 0)
        {
            string authorId = await GetPostAuthorId(postId, isQuestion);
            await _usersService.UpdateUserScoreAsync(new UpdateUserScoreRequest
            {
                UserId = authorId,
                Delta = authorDelta
            });
            _logger.LogInformation("Author:{aId} score updated by delta:{d}", authorId, authorDelta);
        }

        // 7. Update voter score
        if (voterDelta != 0)
        {
            await _usersService.UpdateUserScoreAsync(new UpdateUserScoreRequest
            {
                UserId = userId,
                Delta = voterDelta
            });
            _logger.LogInformation("Voter:{uId} score updated by delta:{d}", userId, voterDelta);
        }

        return vote;
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

    private async Task<bool> IsSelfVote(string userId, string postId, bool isQuestion)
    {
        if (isQuestion)
        {
            var question = await _questionsService.GetQuestionByIdAsync(postId);
            if (question is null)
            {
                _logger.LogInformation("Question:{pId} was not found.", postId);
                throw new NotFoundException("Question not found.");
            }
            return question.AuthorId == userId;
        }
        else
        {
            var comment = await _commentsService.GetCommentByIdAsync(postId);
            if (comment is null) throw new NotFoundException("Comment not found.");
            return comment.AuthorId == userId;
        }
    }

    private async Task<string> GetPostAuthorId(string postId, bool isQuestion)
    {
        if (isQuestion)
        {
            var question = await _questionsService.GetQuestionByIdAsync(postId);
            return question?.AuthorId ?? string.Empty;
        }
        else
        {
            var comment = await _commentsService.GetCommentByIdAsync(postId);
            return comment?.AuthorId ?? string.Empty;
        }
    }

    private int ComputeVoteCountDelta(VoteResult result) => result.Action switch
    {
        VoteAction.Added => result.Vote.IsDownvote ? -1 : 1,
        VoteAction.Removed => result.Vote.IsDownvote ? 1 : -1,
        VoteAction.Switched => result.Vote.IsDownvote ? -2 : 2,
        _ => 0
    };

    private float ComputeVotePoints(bool isDownvote, bool isQuestion) =>
        (isDownvote, isQuestion) switch
        {
            (false, true) => 2.5f,
            (false, false) => 5.0f,
            (true, true) => -1.5f,
            (true, false) => -2.5f
        };

    private float ComputeAuthorDelta(VoteResult result, bool isQuestion)
    {
        float newPoints;
        if (result.Action == VoteAction.Removed)
            newPoints = 0;
        else
            newPoints = ComputeVotePoints(result.Vote.IsDownvote, isQuestion);

        float oldPoints;
        if (result.Action == VoteAction.Added)
            oldPoints = 0;
        else
            oldPoints = ComputeVotePoints(result.PreviousIsDownvote, isQuestion);

        return newPoints - oldPoints;
    }

    private float ComputeVoterDelta(VoteResult result)
    {
        float newPoints;
        if (result.Action != VoteAction.Removed && result.Vote.IsDownvote)
            newPoints = -1.5f;
        else
            newPoints = 0;

        float oldPoints;
        if (result.Action != VoteAction.Added && result.PreviousIsDownvote)
            oldPoints = -1.5f;
        else
            oldPoints = 0;

        return newPoints - oldPoints;
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