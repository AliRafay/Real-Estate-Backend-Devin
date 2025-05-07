namespace Demo.WebApi.Application.Common.FCMNotification;

public interface IFCMNotificationService : ITransientService
{
    Task SendAsync(FCMNotificationRequest request);
}
