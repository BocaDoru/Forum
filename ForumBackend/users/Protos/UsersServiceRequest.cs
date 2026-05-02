using commons.MessageBase;
using commons.Protos;

namespace usersServiceClient;

public partial class UserIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Id) ? "User Id can not be empty." : null;
}

public partial class UserEmailRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Email) ? "User Email can not be empty." : null;
}

public partial class UsernameRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Username))
            return "Username can not be empty.";
        return null;
    }
}

public partial class ChangePasswordRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword))
            return "Old password and new password can not be empty.";
        return null;
    }
}

public partial class RegisterRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Username))
            return "Registration details are missing or incomplete.";

        return null;
    }
}

public partial class AuthRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            return "Email or password are missing.";

        return null;
    }
}

public partial class BanRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(ModeratorId))
            return "You need a moderator account to be able to ban another user.";
        if (string.IsNullOrEmpty(UserId))
            return "No user was selected to be banned.";
        return null;
    }
}

public partial class UnbanRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(ModeratorId))
            return "You need a moderator account to be able to unban another user.";
        if (string.IsNullOrEmpty(UserId))
            return "No user was selected to be unbanned.";
        return null;
    }
}

public partial class UpdateUserScoreRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(UserId) ? "User ID is missing." : null;
}

public partial class EmptyUsersRequest : IRequestBase
{
    public string? Validate() => null;
}