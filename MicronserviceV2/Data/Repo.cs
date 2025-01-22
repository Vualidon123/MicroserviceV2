using JWT_Authen.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public Repository(MongoDbContext context) => _collection = context.GetCollection<T>();

    public async Task<T?> GetByIdAsync(int id)
    {
        var filter = Builders<T>.Filter.Eq("ID", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        var filter = Builders<T>.Filter.Eq("ID", (int)typeof(T).GetProperty("ID").GetValue(entity, null));
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(T entity)
    {
        var filter = Builders<T>.Filter.Eq("ID", (int)typeof(T).GetProperty("ID").GetValue(entity, null));
        await _collection.DeleteOneAsync(filter);
    }

    public Task SaveChangesAsync()
    {
        // MongoDB does not require explicit save changes
        return Task.CompletedTask;
    }
}
