using Demo.WebApi.Application.Common.FCMNotification;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Public;
using Demo.WebApi.Shared.Notifications;
using Functions.Infrastructure.Persistence.Context;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NotificationFunction.Functions;

public class WebNotificationQueueTrigger
{
    private readonly FunctionDbContext _dbContext;
    private readonly IFCMNotificationService _fCMNotificationService;

    public WebNotificationQueueTrigger(FunctionDbContext dbContext, IFCMNotificationService fCMNotificationService)
    {
        this._dbContext = dbContext;
        this._fCMNotificationService = fCMNotificationService;
    }

    [Function(nameof(WebNotificationQueueTrigger))]
    public async Task RunAsync([QueueTrigger(QueueConstants.WebNotificationQueueTrigger)] FCMNotificationRequest request, ILogger log)
    {
        if (!request.IsCustom)
        {
            var template = await _dbContext.NotificationTemplates
                .Include(n => n.Image)
                .FirstOrDefaultAsync(n => n.Code == request.Code);

            if (template == null)
            {
                log.LogError($"Notification template for code {request.Code} not found!");
                return;
            }

            if (request.Data != null)
            {
                template.Title = this.ReplaceTokens(template.Title, request.Data);
                template.Body = this.ReplaceTokens(template.Body, request.Data);
            }

            this.MapTemplateToNotificationRequest(template, request);
        }

        var notification = new Notification
        {
            Title = request.Title!,
            Description = request.Body!,
            ImageId = request.Image?.Id,
            Url = request.Url,
            Code = request.Code,
            IconLink = request.IconLink!
        };

        await _dbContext.Notifications.AddAsync(notification);
        await _dbContext.SaveChangesAsync();

        if (request.IsBatch)
        {
            foreach (FCMToken user in request.Tokens!.DistinctBy(x => new { x.UserId }).ToList())
            {
                await _dbContext.UserNotifications.AddAsync(new UserNotification
                {
                    NotificationId = notification.Id,
                    UserId = user.UserId,
                    Status = NotificationStatus.Unseen
                });
            }
        }
        else
        {
            await _dbContext.UserNotifications.AddAsync(new UserNotification
            {
                NotificationId = notification.Id,
                UserId = request.Token.UserId,
                Status = NotificationStatus.Unseen
            });
        }

        await _dbContext.SaveChangesAsync();
        await _fCMNotificationService.SendAsync(request);
    }

    private string ReplaceTokens(string template, Dictionary<string, string> data)
    {
        foreach (var token in data)
            template = template.Replace($"[{token.Key}]", $"{token.Value}", StringComparison.InvariantCultureIgnoreCase);

        return template;
    }

    private void MapTemplateToNotificationRequest(NotificationTemplate template, FCMNotificationRequest request)
    {
        request.Title = template.Title;
        request.Body = template.Body;
        request.Code = template.Code;
        request.Url = template.Url;
        request.Image.Url = template.Image.AccessURL;
        request.Image.Id = template.Image.Id;
        request.IconLink = template.IconLink;
    }
}
