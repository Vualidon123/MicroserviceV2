using MongoDB.Driver;
using System.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Counter> _counters;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _counters = _database.GetCollection<Counter>("Counters");
    }

    public IMongoCollection<Email> Email => _database.GetCollection<Email>("Email");
    public IMongoCollection<Notification> Notification => _database.GetCollection<Notification>("Notification");

    public int GetNextSequenceValue(string sequenceName)
    {
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, sequenceName);
        var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
        var options = new FindOneAndUpdateOptions<Counter>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var counter = _counters.FindOneAndUpdate(filter, update, options);
        return counter.SequenceValue;
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        if (typeof(T) == typeof(Email))
            return (IMongoCollection<T>)Email;
        if (typeof(T) == typeof(Notification))
            return (IMongoCollection<T>)Notification;
        throw new ArgumentException("Collection not found for the given type");
    }
}

