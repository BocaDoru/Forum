using commons.Tools;
using database;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using users.Models;

namespace users.Implementations;

public interface IUsersService
{
    public Task<User?> GetUserById(string id);
}
public class UsersServiceImplementation(
    ILogger<UsersServiceImplementation> logger,
    IDatabase database
) : IUsersService
{
    private readonly AsyncLazy<IDatabaseCollection<User>> _userCollection = new(() => GetUserCollection(database));
    public async Task<User?> GetUserById(string id)
    {
        var users = await _userCollection;
        return await users.GetOneByIdAsync(id);
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
