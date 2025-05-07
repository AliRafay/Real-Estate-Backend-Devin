using Demo.WebApi.Application.Common.Mailing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Demo.WebApi.Infrastructure.Mailing;
public class SendGridService : IMailService
{
    private readonly MailSettings _settings;
    private readonly ILogger<SendGridService> _logger;
    public SendGridService(IOptions<MailSettings> settings, ILogger<SendGridService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(MailRequest request, CancellationToken ct)
    {
        var tos = new List<EmailAddress>();
        var attachments = new List<SendGrid.Helpers.Mail.Attachment>();
        var from = new EmailAddress(request.From ?? _settings.From, request.DisplayName ?? _settings.DisplayName);

        // To
        foreach (string address in request.To)
            tos.Add(MailHelper.StringToEmailAddress(address.Trim()));

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, request.Subject, !request.IsHtml ? request.Body : string.Empty, request.IsHtml ? request.Body : string.Empty);

        // Cc
        if (request.Cc != null)
        {
            foreach (string? address in request.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                msg.AddCc(address.Trim());
        }

        // Bcc
        if (request.Bcc != null)
        {
            foreach (string address in request.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                msg.AddBcc(address.Trim());
        }

        // Create the file attachments for this e-mail msg
        if (request.AttachmentData != null)
        {
            foreach (var attachmentInfo in request.AttachmentData)
                msg.AddAttachment(attachmentInfo.Key, Convert.ToBase64String(attachmentInfo.Value));
        }

        var client = new SendGridClient(_settings.SendGridApiKey);
        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = await response.Body.ReadAsStringAsync();
            _logger.LogError($"Email failed to deliver to {request.To} due to {errorMessage}.");
            throw new Exception($"Email failed to deliver to {request.To} due to {errorMessage}.");
        }

        _logger.LogInformation($"Message Queue trigger response: {JsonConvert.SerializeObject(response.StatusCode)}");
    }
}
