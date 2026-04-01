using commons.MessageBase;

namespace commentsServiceClient;

public partial class CreateCommentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(AuthorId))
            return "Comment is missing author or question ID.";
        if (string.IsNullOrEmpty(Text))
            return "Comment text cannot be empty.";
        return null;
    }
}

public partial class EditCommentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CommentId) || string.IsNullOrEmpty(AuthorId))
            return "Comment metadata are missing.";
        if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(ImageId))
            return "No changes were made to the comment.";
        return null;
    }
}

public partial class DeleteCommentRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(CommentId) || string.IsNullOrEmpty(AuthorId))
            return "Comment metadata are missing.";
        return null;
    }
}

public partial class GetCommentsRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(QuestionId))
            return "Question ID is missing.";
        return null;
    }
}