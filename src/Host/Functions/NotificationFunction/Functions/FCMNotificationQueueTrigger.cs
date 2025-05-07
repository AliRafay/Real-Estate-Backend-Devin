using Demo.WebApi.Application.Common.FCMNotification;
using Demo.WebApi.Shared.Notifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NotificationFunction.Settings;

namespace NotificationFunction.Functions;

public class FCMNotificationQueueTrigger
{
    private readonly FCMSettings _configuration;
    private FirebaseApp _firebaseApp;
    private FirebaseMessaging _firebaseMessaging;

    public FCMNotificationQueueTrigger(IOptions<FCMSettings> options)
    {
        _configuration = options.Value;
    }

    [Function(nameof(FCMNotificationQueueTrigger))]
    public async Task RunAsync([QueueTrigger(QueueConstants.FCMNotificationQueueTrigger)] FCMNotificationRequest request, ILogger log)
    {
        _firebaseApp = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(
            new AppOptions
            {
                Credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(_configuration))
            });

        _firebaseMessaging = FirebaseMessaging.GetMessaging(_firebaseApp);

        if (request.IsBatch)
            await SendBatchNotifications(request, log);
        else
            await SendSingleNotification(request, log);
    }

    private async Task SendBatchNotifications(FCMNotificationRequest request, ILogger log)
    {
        int batchSize = 500;

        List<string?> recipientTokens = request.Tokens!.Select(x => x.Value).ToList();

        if (recipientTokens.Count == 0)
            return;

        for (int i = 0; i < recipientTokens.Count; i += batchSize)
        {
            List<string?> currentBatch = recipientTokens.Skip(i).Take(batchSize).ToList();

            var message = new MulticastMessage()
            {
                Notification = request.IsSilent ? null : new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.Image.Url
                },
                Tokens = currentBatch,
                Data = request.Data
            };

            var response = await _firebaseMessaging.SendMulticastAsync(message);
            log.LogInformation($"FCM batch notification response: {response}");
        }
    }

    private async Task SendSingleNotification(FCMNotificationRequest request, ILogger log)
    {
        var message = new Message()
        {
            Notification = request.IsSilent ? null : new Notification
            {
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.Image.Url,
            },
            Data = request.Data
        };

        if (!request.Token!.Value.IsNullOrEmpty())
        {
            message.Token = request.Token.Value;
        }
        else if (!request.Topic.IsNullOrEmpty())
        {
            message.Topic = request.Topic;
        }
        else
        {
            message.Topic = "all";
        }

        var result = await _firebaseMessaging.SendAsync(message);
        log.LogInformation($"FCM queue trigger response: {result}");
    }
}
