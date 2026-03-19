
namespace database;

public interface IDatabase
{
    public IDatabaseCollection<T> GetCollection<T>() where T : DatabaseModel;
}
