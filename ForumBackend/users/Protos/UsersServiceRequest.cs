using commons.MessageBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace usersServiceClient;

public partial class UserIdRequest : IRequestBase
{
    public string? Validate() => string.IsNullOrEmpty(Id) ? "User Id can not be empty." : null;
}
