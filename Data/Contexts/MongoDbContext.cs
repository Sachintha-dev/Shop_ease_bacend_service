using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using ECommerceBackend.Models;

namespace ECommerceBackend.Data.Contexts
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<DatabaseSettings> dbSettings, ILogger<MongoDbContext> logger)
        {
            _logger = logger;

            try
            {
                var client = new MongoClient(dbSettings.Value.ConnectionString);
                _database = client.GetDatabase(dbSettings.Value.DatabaseName);

                // Ping the database to check the connection
                _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

                 Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
                _logger.LogInformation("Successfully connected to MongoDB database: {DatabaseName}", dbSettings.Value.DatabaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while connecting to the MongoDB database.");
                Console.WriteLine(ex);
                throw;
            }
        }

       
    }
}
