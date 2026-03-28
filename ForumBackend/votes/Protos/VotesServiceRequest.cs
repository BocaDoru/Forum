using commons.MessageBase;

namespace votesServiceClient;

public partial class CastVoteRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(UserId)) return "UserId cannot be empty.";
        if (string.IsNullOrEmpty(PostId)) return "PostId cannot be empty.";
        if (Type != 0 && Type != 1) return "Type must be 0 (question) or 1 (answer).";
        if (VoteValue != 0 && VoteValue != 1) return "VoteValue must be 0 (downvote) or 1 (upvote).";
        return null;
    }

}

public partial class GetVoteCountRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(PostId)) return "PostId cannot be empty.";
        if (Type != 0 && Type != 1) return "Type must be 0 (question) or 1 (answer).";
        return null;
    }
}