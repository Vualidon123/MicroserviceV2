using MongoDB.Driver;
using System.Linq.Expressions;


public interface RabitIRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
public class Repository<T> : RabitIRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;
    private readonly MongoDbContext _context;

    public Repository(MongoDbContext context)
    {
        _context = context;
        _collection = context.GetCollection<T>();
    }

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
        var idProperty = typeof(T).GetProperty("ID") ?? typeof(T).GetProperty("_ID");
        if (idProperty != null && idProperty.PropertyType == typeof(int))
        {
            var newId = _context.GetNextSequenceValue(typeof(T).Name);
            idProperty.SetValue(entity, newId);  
        }
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