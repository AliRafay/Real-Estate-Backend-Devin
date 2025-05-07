using Demo.WebApi.Application.Common.Mailing;
using Demo.WebApi.Shared.Notifications;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationFunction.Settings;

namespace NotificationFunction.Functions;

public class MailQueueTrigger
{
    private readonly MailSettings _mailConfig;

    public MailQueueTrigger(IOptions<MailSettings> mailConfig)
    {
        this._mailConfig = mailConfig.Value;
    }

    [Function(nameof(MailQueueTrigger))]
    public async Task RunAsync([QueueTrigger(QueueConstants.MailQueueTrigger)] MailRequest request, ILogger log)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_mailConfig.DisplayName, request.From ?? _mailConfig.From));

            foreach (string address in request.To)
            {
                email.To.Add(MailboxAddress.Parse(address));
            }

            if (!string.IsNullOrEmpty(request.ReplyTo))
            {
                email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));
            }

            if (request.Bcc != null)
            {
                foreach (string address in request.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                    email.Bcc.Add(MailboxAddress.Parse(address.Trim()));
            }

            if (request.Cc != null)
            {
                foreach (string? address in request.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                    email.Cc.Add(MailboxAddress.Parse(address.Trim()));
            }

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                    email.Headers.Add(header.Key, header.Value);
            }

            var builder = new BodyBuilder();

            email.Sender = new MailboxAddress(request.DisplayName ?? _mailConfig.DisplayName, request.From ?? _mailConfig.From);
            email.Subject = request.Subject;
            builder.HtmlBody = request.Body;

            if (request.AttachmentData != null)
            {
                foreach (var attachmentInfo in request.AttachmentData)
                    builder.Attachments.Add(attachmentInfo.Key, attachmentInfo.Value);
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            await smtp.ConnectAsync(_mailConfig.Host, _mailConfig.Port, false);
            await smtp.AuthenticateAsync(_mailConfig.UserName, _mailConfig.Password);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
        }
    }
}
