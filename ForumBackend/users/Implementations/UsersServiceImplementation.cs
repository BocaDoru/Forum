using commons.Tools;
using database;
using MongoDB.Driver;
using security;
using System.Text.RegularExpressions;
using users.Models;
using commons.MessageBase;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace users.Implementations;

public interface IUsersService
{
    public Task<User?> GetUserById(string id);

    public Task<UserWithJwt> Auth(string email, string password);

    public Task<UserWithJwt> Register(string username, string email, string password, bool isModerator);
}
public class UsersServiceImplementation(
    ILogger<UsersServiceImplementation> logger,
    IDatabase database,
    IConfiguration configuration
) : IUsersService
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    private readonly AsyncLazy<IDatabaseCollection<User>> _userCollection = new(() => GetUserCollection(database));

    public async Task<User?> GetUserById(string id)
    {
        var users = await _userCollection;
        return await users.GetOneByIdAsync(id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var users = await _userCollection;
        return await users.GetOneAsync(u => u.Email == email);
    }

    public async Task<UserWithJwt> Auth(string email, string password)
    {
        User? user = await GetUserByEmail(email);
        if (user is null)
        {
            _logger.LogInformation("User with email:{email} was not found.", email);
            throw new BadRequestException("Invalid email or password.");
        }

        if (user.IsBanned)
        {
            _logger.LogInformation("User with email:{email} is banned.", email);
            throw new ForbiddenException("Account was banned.");
        }

        if (!VerifyPassword(password, user.Password))
        {
            _logger.LogInformation("Invalid password for email:{email}.", email);
            throw new BadRequestException("Invalid email or password.");
        }

        var jwtToken = GenerateJwtToken(user.Id, user.Username, email, user.IsModerator);

        _logger.LogInformation("User:{user} successfuly log in with token:{jwt}", user, jwtToken);
        return new UserWithJwt(user, jwtToken);
    }

    public async Task<UserWithJwt> Register(string username, string email, string password, bool isModerator)
    {
        if (!IsValidEmail(email) || !IsValidPassword(password))
        {
            _logger.LogInformation("Invalid email:{email} or password:{password}.", email, password);
            throw new BadRequestException("Invalid email or password.");
        }

        var users = await _userCollection;

        if (await GetUserByEmail(email) is not null)
        {
            _logger.LogInformation("Email:{email} is already used.", email);
            throw new BadRequestException("Email is already used.");
        }

        string passwordHash = HashPassword(password);

        User newUser = new User
        {
            Username = username,
            Email = email,
            Password = passwordHash,
            IsModerator = isModerator,
            IsBanned = false,
            Score = 0
        };

        newUser.Id = await users.InsertAsync(newUser);

        var jwtToken = GenerateJwtToken(newUser.Id, username, email, isModerator);

        _logger.LogInformation("User:{user} successfuly registered with token:{jwt}", newUser, jwtToken);
        return new UserWithJwt(newUser, jwtToken);
    }


    private bool IsValidEmail(string email)
    {

        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
    }

    private bool IsValidPassword(string password)
    {
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[{\]};:'"",<.>/?\\|`~]).{8,}$";
        return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, passwordPattern);
    }

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, 13);

    private bool VerifyPassword(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    private string GenerateJwtToken(string id, string username, string email, bool isModerator)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_configuration["SecretKey"] ?? "a_very_secret_key_that_must_be_long_and_complex");

        var claims = new[]
        {
            new Claim(UserJwtExtensions.IdClaim, id),
            new Claim(UserJwtExtensions.NameClaim, username),
            new Claim(UserJwtExtensions.EmailClaim, email),
            new Claim(UserJwtExtensions.IsModeratorClaim, isModerator ? "true" : "false"),
            new Claim(UserJwtExtensions.IsBannedClaim, "false")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    internal static async Task<IDatabaseCollection<User>> GetUserCollection(IDatabase database)
    {
        var collection = database.GetCollection<User>();
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<User>(emailIndex, new CreateIndexOptions { Name = "UserEmailIndex", Unique = true })
        ]);

        return collection;
    }
}
