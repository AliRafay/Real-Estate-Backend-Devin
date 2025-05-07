using Demo.WebApi.Application.Common.FCMNotification;
using Demo.WebApi.Application.Storage;
using Demo.WebApi.Shared.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Shared.Services.FCMNotification;

public class FCMNotificationService : IFCMNotificationService
{
    private readonly IStorageQueueClient<FCMNotificationRequest> _queueClient;

    public FCMNotificationService(IStorageQueueClient<FCMNotificationRequest> queueClient)
    {
        _queueClient = queueClient;
    }

    public async Task SendAsync(FCMNotificationRequest request)
    {
        await _queueClient.InsertAsync(request, QueueConstants.FCMNotificationQueueTrigger, request.ScheduleAt);
    }
}
