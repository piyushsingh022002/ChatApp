// Services/MongoService.cs
using MongoDB.Driver;

namespace ChatApp.Api.Services;

public class MongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(dbName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);
}
