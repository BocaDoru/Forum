using MongoDB.Driver;
using System.Linq.Expressions;

namespace database;

public interface IDatabaseCollection<T> where T : DatabaseModel
{
    IMongoCollection<T> MongoCollection {  get; }

    Task<T> GetOneByIdAsync(string id);
    Task<T> GetOneAsync(Expression<Func<T, bool>> predicate);
    Task<string> InsertAsync(T entity);
    Task ReplaceAsync(T entity);
    Task ReplaceAsync(Expression<Func<T, bool>> predicate, T entity);
    Task UpsertAsync(Expression<Func<T, bool>> predicate, T entity);
    Task<bool> DeleteWithIdAsync(string id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task UpdateAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> selector, TProperty value);
    Task UpdateByIdAsync<TProperty>(string id, Expression<Func<T, TProperty>> selector, TProperty value);

}
