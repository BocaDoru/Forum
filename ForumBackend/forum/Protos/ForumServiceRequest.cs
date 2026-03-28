using commons.MessageBase;
using DnsClient.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace forumServiceClient;

public partial class CreateQuestionRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(AuthorId))
            return "Question is missing author.";
        if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Text) || TagIds is null || TagIds.Count < 1)
            return "Question details are incomplete";
        return null;
    }
}

public partial class EditQuestionRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(UserId))
            return "Question metadata are missing.";
        if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Text) || TagIds is null || TagIds.Count < 1 || string.IsNullOrEmpty(Status))
            return "Question details are incomplete";
        return null;
    }
}

public partial class DeleteQuestionRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(UserId))
            return "Question metadata are missing.";
        return null;
    }
}

public partial class QuestionIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(QuestionId) ? "Question ID is missing from request." : null;
}

public partial class QuestionsRequest : IRequestBase
{
    public string? Validate()
    {
        if (Skip < 0)
        {
            return "Skip can not be less than 0";
        }

        if (Limit < 1 || Limit > 20)
        {
            return "Limit needs to be between 1 and 20";
        }

        return null;
    }
}

public partial class QuestionsByAuthorRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(AuthorId))
            return "Question author ID is missing from request.";

        return Pagination.Validate();
    }
}

public partial class QuestionsByTitleRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Title))
            return "Question title is missing from request.";

        return Pagination.Validate();
    }
}

public partial class QuestionsByTagRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Tag))
            return "Question tag is missing from request.";

        return Pagination.Validate();
    }
}

public partial class CreateTagRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Name) ? "Tag can not be empty." : null;
}

public partial class TagNameRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Name) ? "Tag can not be empty." : null;
}

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
        if (string.IsNullOrEmpty(Text))
            return "Comment text cannot be empty.";
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
