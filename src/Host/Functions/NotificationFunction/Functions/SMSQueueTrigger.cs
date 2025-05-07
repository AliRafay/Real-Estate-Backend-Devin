using Demo.WebApi.Application.Common.Messaging;
using Demo.WebApi.Shared.Notifications;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NotificationFunction.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationFunction.Functions;

public class SMSQueueTrigger
{
    private readonly MessageSettings _messageConfig;

    public SMSQueueTrigger(IOptions<MessageSettings> messageConfig)
    {
        this._messageConfig = messageConfig.Value;
    }

    [Function(nameof(SMSQueueTrigger))]
    public async Task RunAsync([QueueTrigger(QueueConstants.SMSQueueTrigger)] MessageRequest request, ILogger log)
    {
        string? fromNumber = _messageConfig.From;
        TwilioClient.Init(_messageConfig.AccountSID, _messageConfig.AuthToken);

        var messageOptions = new CreateMessageOptions(new PhoneNumber(request.To));
        messageOptions.From = new PhoneNumber(fromNumber);
        messageOptions.Body = request.Body;

        var response = await MessageResource.CreateAsync(messageOptions);
        var message = await MessageResource.FetchAsync(pathSid: response.Sid);

        if (message.Status == MessageResource.StatusEnum.Failed || message.Status == MessageResource.StatusEnum.Undelivered)
        {
            log.LogError($"Message failed to deliver to {request.To} due to {message.ErrorMessage}.");
            throw new Exception($"Message failed to deliver to {request.To} due to {message.ErrorMessage}.");
        }

        log.LogInformation($"SMS Queue trigger function result: {JsonConvert.SerializeObject(message.Status)}");
    }
}