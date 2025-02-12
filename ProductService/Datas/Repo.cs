using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Datas
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(ObjectId id);
        Task<T> GetByIdAsync(int id); // New method
        Task AddAsync(T entity);
        Task UpdateAsync(int id, T entity);
        Task DeleteAsync(ObjectId id);
    }

    public class Repository<T> : IRepository<T>
    {
        private readonly IMongoCollection<T> _collection;

        public Repository(MongoDbContext context)
        {
            _collection = context.GetCollection<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetByIdAsync(ObjectId id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("ProductId", id)).FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(int id, T entity)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("ProductId", id), entity);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        }
    }

    public class OrderRepository : Repository<Order>
    {
        public OrderRepository(MongoDbContext context) : base(context) { }
    }

    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(MongoDbContext context) : base(context) { }
    }

    public class OrderDetailRepository : Repository<OrderDetail>
    {
        public OrderDetailRepository(MongoDbContext context) : base(context) { }
    }
}
