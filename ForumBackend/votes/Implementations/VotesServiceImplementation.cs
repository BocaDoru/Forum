using database;
using votes.Models;
using commons.MessageBase;
using commons.Protos;
using MongoDB.Driver;
using security;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace votes.Implementations;

public interface IVotesService
{
    Task<(bool success, string message, int newVoteCount)> CastVote(
        string userId,
        string postId,
        int type,
        int voteValue
        );
    Task<int> GetVoteCount(string postId, int type);
}

public class VotesServiceImplementation( ILogger<VotesServiceImplementation> logger, IDatabase database) : IVotesService
{
    private readonly AsyncLazy<IDatabaseCollection<Vote>> _votesCollection = 
        new(() => Task.FromResult(database.GetCollection<Vote>()));
    
    public async Task<(bool success, string message, int newVoteCount)> CastVote(string userId, string postId, int type, int voteValue)
    {
        var votes = await _votesCollection;

        var existingVote = await votes.GetOneAsync(v => v.UserId == userId && v.PostId == postId && v.Type == type);

        if (existingVote is null)
        {
            // New vote -> Insert
            await votes.InsertAsync(new Vote
            { 
                UserId = userId,
                PostId = postId, 
                Type = type, 
                VoteValue = voteValue 
            });
        }
        else if (existingVote.VoteValue == voteValue)
        {
            // Same vote again -> toggle off (delete)
            await votes.DeleteWithIdAsync(existingVote.Id);
        }
        else
        {
            // Different vote -> update
            await votes.UpdateByIdAsync(existingVote.Id, v => v.VoteValue, voteValue);


        }
        int newVoteCount = await GetVoteCount(postId, type);
        return (true, "Vote updated successfully.", newVoteCount);

    }

    public async Task<int> GetVoteCount(string postId, int type)
    {
        var votes = await _votesCollection;

        var allVotes = await votes.MongoCollection
            .Find(v => v.PostId == postId && v.Type == type)
            .ToListAsync();

        int upvotes = allVotes.Count(v => v.VoteValue == 1);
        int downvotes = allVotes.Count(v => v.VoteValue == 0);

        return upvotes - downvotes;
    }


}