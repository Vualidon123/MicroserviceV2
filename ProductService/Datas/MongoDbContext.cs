using MongoDB.Bson;
using MongoDB.Driver;

namespace ProductService.Datas
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Sequence> _sequences;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _sequences = _database.GetCollection<Sequence>("Sequences");
        }

        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
        public IMongoCollection<OrderDetail> OrderDetails => _database.GetCollection<OrderDetail>("OrderDetails");
        public IMongoCollection<T> GetCollection<T>()
        {
            if (typeof(T) == typeof(Order))
                return (IMongoCollection<T>)Orders;
            if (typeof(T) == typeof(Product))
                return (IMongoCollection<T>)Products;
            if (typeof(T) == typeof(OrderDetail))
                return (IMongoCollection<T>)OrderDetails;
            throw new ArgumentException("Collection not found for the given type");
        }
        public async Task<int> GetNextSequenceValue(string sequenceName)
        {
            var filter = Builders<Sequence>.Filter.Eq(s => s.Name, sequenceName);
            var update = Builders<Sequence>.Update.Inc(s => s.Value, 1);
            var options = new FindOneAndUpdateOptions<Sequence>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var sequence = await _sequences.FindOneAndUpdateAsync(filter, update, options);
            return sequence.Value;
        }
        public void SeedData()
        {
            // Seed Order data
       
            // Seed Product data
            if (!Products.Find(_ => true).Any())
            {
                var product1Id = ObjectId.GenerateNewId();
                var product2Id = ObjectId.GenerateNewId();
                var product3Id = ObjectId.GenerateNewId();
                Products.InsertMany(new[]
                {
                        new Product
                        {
                            ObjectId = product1Id,
                            ProductId = 1,
                            Price = 19.99m,
                            ProductName="Test1",
                            Quantity=100   
                        },
                        new Product
                        {
                            ObjectId = product2Id,
                            ProductId = 2,
                            Price = 29.99m,
                            ProductName="Test2",
                            Quantity=250
                        },
                        new Product
                        {
                            ObjectId = product3Id,
                            ProductId = 3,
                            Price = 39.99m,
                            ProductName="Test3",
                            Quantity=25
                        }
                    });
            }

            // Seed OrderDetail data
        }
    }
  
}
