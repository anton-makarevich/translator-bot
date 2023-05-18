using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;
using Sanet.Bots.Telegram.Services;

namespace Sanet.Bots.Telegram.Azure.Services;

public class CosmosService : IDatabaseService
{
    private readonly string _databaseId;
    private readonly string _containerId;
    private readonly ILogger _logger;
    private readonly CosmosClient _cosmosClient;

    public CosmosService(string connectionString, ILogger logger)
    {
        _databaseId = "sanet-bot";
        _containerId = "translator-subscriptions";
        _logger = logger;
        
        _cosmosClient = new CosmosClient(connectionString);
    }

    public async Task<bool> SaveSubscription(SubscriptionEntity item)
    {
        var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
        var container = await database.Database.CreateContainerIfNotExistsAsync(_containerId, "/groupId");
        
        // Create a composite key by concatenating groupId and chatId
        var compositeKey = $"{item.GroupId}_{item.ChatId}";

        var itemToSave = new
        {
            id = compositeKey,
            groupId=item.GroupId,
            chatId=item.ChatId,
        };

        var partitionKey = new PartitionKey(item.GroupId);
        var response = await container.Container.CreateItemAsync(item, partitionKey);

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            _logger.LogInformation("Object saved successfully!");
            return true;
        }

        _logger.LogError("{StatusCode}, {Message}",response.StatusCode,"Failed to save the object.");
        return false;
    }
    
    public async Task<IEnumerable<SubscriptionEntity>> QueryByGroupIdAsync(ulong groupId)
    {
        var database = _cosmosClient.GetDatabase(_databaseId);
        var container = database.GetContainer(_containerId);

        const string queryText = "SELECT * FROM c WHERE c.groupId = @groupId";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@groupId", groupId);

        var queryIterator = container.GetItemQueryIterator<SubscriptionEntity>(queryDefinition);

        var results = new List<SubscriptionEntity>();
        while (queryIterator.HasMoreResults)
        {
            var response = await queryIterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results;
    }
}