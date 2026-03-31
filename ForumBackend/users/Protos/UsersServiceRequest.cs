using commons.MessageBase;
using commons.Protos;

namespace usersServiceClient;

public partial class UserIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Id) ? "User Id can not be empty." : null;
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

public partial class UpdateUserScoreRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(UserId) ? "User ID is missing." : null;
}

