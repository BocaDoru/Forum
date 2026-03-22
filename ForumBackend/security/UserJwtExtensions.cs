using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace security;
public record UserJwt
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required bool IsModerator { get; set; } = false;
    public bool IsBanned { get; set; } = false;
}

public static class UserJwtExtensions
{
    public const string IdClaim = JwtRegisteredClaimNames.Sub;
    public const string NameClaim = JwtRegisteredClaimNames.Name;
    public const string EmailClaim = JwtRegisteredClaimNames.Email;
    public const string IsModeratorClaim = "isModerator";
    public const string IsBannedClaim = "isBanned";

    private static readonly JwtSecurityTokenHandler s_jwtHandler = new();

    public static IEnumerable<Claim> ToClaims(this UserJwt jwt)
    {
        return [
            new Claim(IdClaim, jwt.Id),
            new Claim(NameClaim, jwt.Name),
            new Claim(EmailClaim, jwt.Email),
            new Claim(IsModeratorClaim, jwt.IsModerator.ToString().ToLowerInvariant()),
            new Claim(IsBannedClaim, jwt.IsBanned.ToString().ToLowerInvariant())
        ];
    }

    public static UserJwt FromClaims(IEnumerable<Claim> claims)
    {
        return new UserJwt
        {
            Id = GetClaims(claims, IdClaim),
            Name = GetClaims(claims, NameClaim),
            Email = GetClaims(claims, EmailClaim),
            IsModerator = GetClaims(claims, IsModeratorClaim) == "true",
            IsBanned = GetClaims(claims, IsBannedClaim) == "true"
        };
    }

    public static UserJwt FromBearerToken(string token)
    {
        var claimPrincipal = s_jwtHandler.ReadJwtToken(token);
        return FromClaims(claimPrincipal.Claims);
    }

    public static string GetClaims(IEnumerable<Claim> claims, string claimType)
    {
        return claims.Single(c => c.Type == claimType).Value;
    }
}
