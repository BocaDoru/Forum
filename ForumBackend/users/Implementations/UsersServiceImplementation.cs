using commons.MessageBase;
using database;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using users.Models;

namespace users.Implementations;

public interface IUsersService
{
    Task<User?> GetUserById(string id);
    Task<User?> GetUserByEmail(string email);

    Task<UserWithJwt> Auth(string email, string password);

    Task<UserWithJwt> Register(string username, string email, string password, bool isModerator);

    Task<User> UpdateUsernameAsync(string userId, string username);
    Task DeleteUserAsync(string userId);
    Task ChangePasswordAsync(string userId, string oldPassword, string newPassword);

    Task BanUserAsync(string moderatorId, string userId, string? reason);
    Task UnbanUserAsync(string moderatorId, string userId);

    public Task<User> UpdateScore(string userId, float delta);

    public Task<float> GetUserScore(string userId);
    Task<List<User>> GetAllUsersAsync();
}
public class UsersServiceImplementation(
    ILogger<UsersServiceImplementation> logger,
    IDatabase database,
    IConfiguration configuration
) : IUsersService
{
    private readonly ILogger<UsersServiceImplementation> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    private readonly AsyncLazy<IDatabaseCollection<User>> _users = new(() => GetUserCollection(database));

    public async Task<User?> GetUserById(string id)
    {
        var users = await _users;
        return await users.GetOneByIdAsync(id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var users = await _users;
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

        var users = await _users;

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

    public async Task<User> UpdateUsernameAsync(string userId, string username)
    {
        var user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogInformation("User with id:{id} does not exist", userId);
            throw new BadRequestException("User not found.");
        }

        user.Username = username;

        var users = await _users;
        await users.ReplaceAsync(user);

        return user;
    }

    public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogInformation("User with id:{id} does not exist", userId);
            throw new BadRequestException("User not found.");
        }

        if (!VerifyPassword(oldPassword, user.Password))
        {
            _logger.LogInformation("Incorrect password for user id:{id}", userId);
            throw new BadRequestException("Invalid email or password.");
        }

        user.Password = HashPassword(newPassword);

        var users = await _users;
        await users.ReplaceAsync(user);
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogInformation("User with id:{id} does not exist", userId);
            throw new BadRequestException("User not found.");
        }

        var users = await _users;
        await users.DeleteWithIdAsync(userId);
    }

    public async Task BanUserAsync(string moderatorId, string userId, string? reason)
    {
        var moderator = await GetUserById(moderatorId);
        if (moderator is null || !moderator.IsModerator)
        {
            _logger.LogError("Invalid moderator account for id:{id}", moderatorId);
            throw new ForbiddenException("Only moderators have permisssion to ban other users.");
        }

        var user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogError("User with id:{id} can not be found", userId);
            throw new NotFoundException("User can not be found.");
        }

        user.IsBanned = true;
        var users = await _users;
        await users.ReplaceAsync(user);

        await SendBannedEmail(user, moderator, reason);
        _logger.LogInformation("User:{userId} was banned by moderator:{moderatorId} for {reason}", userId, moderatorId, reason ?? "Bad behaviour");
    }

    public async Task UnbanUserAsync(string moderatorId, string userId)
    {
        var moderator = await GetUserById(moderatorId);
        if (moderator is null || !moderator.IsModerator)
        {
            _logger.LogError("Invalid moderator account for id:{id}", moderatorId);
            throw new ForbiddenException("Only moderators have permisssion to unban other users.");
        }

        var user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogError("User with id:{id} can not be found", userId);
            throw new NotFoundException("User can not be found.");
        }

        if (!user.IsBanned)
        {
            _logger.LogInformation("User was not banned.");
            return;
        }

        user.IsBanned = false;
        var users = await _users;
        await users.ReplaceAsync(user);

        _logger.LogInformation("User:{userId} was unbanned by moderator:{moderatorId}", userId, moderatorId);
    }

    public async Task<User> UpdateScore(string userId, float delta)
    {
        var users = await _users;

        User? user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogInformation("User:{id} was not found.", userId);
            throw new NotFoundException("User not found.");
        }

        float newScore = user.Score + delta;

        await users.UpdateByIdAsync(userId, u => u.Score, newScore);

        _logger.LogInformation("User:{uId} score updated by delta:{delta}, new score:{score}", userId, delta, newScore);
        return user with { Score = newScore };
    }

    public async Task<float> GetUserScore(string userId)
    {
        User? user = await GetUserById(userId);
        if (user is null)
        {
            _logger.LogInformation("User:{id} was not found.", userId);
            throw new NotFoundException("User not found.");
        }

        return user.Score;
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

    private async Task SendBannedEmail(User bannedUser, User moderator, string? reason)
    {
        try
        {
            var httpContent = BuildBrevoPayload(bannedUser.Email, bannedUser.Username, "BannedEmail", new
            {
                banned_username = bannedUser.Username,
                ban_reason = reason ?? "Bad behavior.",
                moderator_name = moderator.Username,
                moderator_email = moderator.Email,
            });
            await SendEmailToBrevoAsync(httpContent, bannedUser.Email);
        }
        catch (ServiceMessageException ex)
        {
            _logger.LogError(ex, "Failed to send email due to an unexpected error");
            throw new InternalErrorException("Failed to send banned email to user.");
        }
    }

    private StringContent BuildBrevoPayload(string toEmail, string toName, string templateName, object templateData)
    {
        string rawHtml = GetTemplateContent(templateName);

        string emailSubject = ExtractSubjectFromTemplate(rawHtml);
        string htmlWithData = PopulateTemplate(rawHtml, templateData);

        var payload = new
        {
            sender = new
            {
                email = _configuration["BrevoSettings:EmailFrom"],
                name = _configuration["BrevoSettings:SenderName"]
            },
            to = new[] {
                new {
                    email = toEmail,
                    name = toName
                }
            },
            subject = emailSubject,
            htmlContent = htmlWithData
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }

    private string GetTemplateContent(string templateName)
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Templates", $"{templateName}.html");
            if (!File.Exists(path))
            {
                _logger.LogError("Template file not found: {TemplatePath}", path);
                throw new InternalErrorException($"Template file '{templateName}.html' not found.");
            }

            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read template file {TemplateName}", templateName);
            throw new InternalErrorException($"Error reading template: {ex.Message}");
        }
    }

    private string ExtractSubjectFromTemplate(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<!--\s*SUBJECT:\s*(.*?)\s*-->", RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        _logger.LogWarning("Template is missing a subject comment. Using default subject.");
        throw new InternalErrorException("Template file is invalid: Subject comment is missing.");
    }

    private string PopulateTemplate(string htmlContent, object templateData)
    {
        if (templateData == null || string.IsNullOrEmpty(htmlContent))
        {
            return htmlContent;
        }

        var properties = templateData.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(templateData, null)?.ToString() ?? string.Empty;

            htmlContent = htmlContent.Replace($"{{{{{prop.Name.ToLower()}}}}}", value);
        }

        return htmlContent;
    }

    private async Task SendEmailToBrevoAsync(StringContent content, string toEmail)
    {
        var apiKey = _configuration["BrevoSettings:ApiKey"];
        var apiUrl = _configuration["BrevoSettings:ApiUrl"];

        HttpClient httpClient = new();

        httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("accept", "application/json");

        var brevoResponse = await httpClient.PostAsync(apiUrl, content);

        if (brevoResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email succesfully sent to {Email}", toEmail);
        }
        else
        {
            var errorBody = await brevoResponse.Content.ReadAsStringAsync();
            _logger.LogError("Brevo error(code {StatusCode}):{ErrorBody}", brevoResponse.StatusCode, errorBody);

            throw new InternalErrorException($"Brevo error: {errorBody}");
        }
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

    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = await _users;
        return await users.MongoCollection.Find(_ => true).ToListAsync();
    }
}
