using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerceBackend.Models;

namespace ECommerceBackend.Data.Contexts
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            _database = client.GetDatabase(dbSettings.Value.DatabaseName);
        }

    }
}
