using commons.MessageBase;

namespace forumServiceClient;

public partial class CastVoteRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(UserId)) return "UserId cannot be empty.";
        if (string.IsNullOrEmpty(PostId)) return "PostId cannot be empty.";
        return null;
    }
}

public partial class GetVoteCountRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(PostId)) return "PostId cannot be empty.";
        return null;
    }
}