using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Demo.WebApi.Application.Storage;

namespace Shared.Services.AzureStorage;

public class AzureQueueClient<T> : IStorageQueueClient<T>
{
    private readonly string connectionString;

    public AzureQueueClient(IConfiguration configuration)
    {
        connectionString = configuration["AzureStorageSettings:ConnectionString"];
    }

    public async Task<string> InsertAsync(T data, string queueName, DateTime? visibilityTimeOut = null, DateTime? executionTime = null)
    {
        QueueClientOptions queueOptions = new() { MessageEncoding = QueueMessageEncoding.Base64 };

        var queueClient = new QueueClient(connectionString, queueName, queueOptions);

        await queueClient.CreateIfNotExistsAsync();

        var message = JsonConvert.SerializeObject(data);
        var res = await queueClient.SendMessageAsync(message, visibilityTimeout: visibilityTimeOut == null ? null : visibilityTimeOut - DateTime.UtcNow, timeToLive: executionTime == null ? null : executionTime - DateTime.UtcNow);

        return res?.Value?.MessageId;
    }

    public async Task<T> PeekAsync(string queueName)
    {
        QueueClientOptions queueOptions = new() { MessageEncoding = QueueMessageEncoding.None };
        var queueClient = new QueueClient(connectionString, queueName, queueOptions);
        var peekedMessage = await queueClient.PeekMessageAsync();

        return JsonConvert.DeserializeObject<T>(peekedMessage.Value.Body.ToString());
    }

    public async Task<T> DequeueAsync(string queueName)
    {
        QueueClientOptions queueOptions = new() { MessageEncoding = QueueMessageEncoding.None };
        var queueClient = new QueueClient(connectionString, queueName, queueOptions);
        var message = await queueClient.ReceiveMessageAsync();

        await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);

        return JsonConvert.DeserializeObject<T>(message.Value.Body.ToString());
    }
}