using System;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Mesawer.InfrastructureLayer.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly IRazorRendererService        _renderer;
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly SendGridConfig               _config;

        public SendGridEmailSender(
            IRazorRendererService renderer,
            IOptions<SendGridConfig> options,
            ILogger<SendGridEmailSender> logger)
        {
            _renderer = renderer;
            _logger   = logger;
            _config   = options.Value;
        }

        public async Task SendAsync(string type, string name, string email, string subject, EmailTemplate template)
        {
            var client   = new SendGridClient(_config.ApiKey);
            var from     = new EmailAddress(_config.FromEmail, _config.FromName);
            var to       = new EmailAddress(email, name);
            var body     = await _renderer.RenderAsync("Basic.cshtml", template);
            var msg      = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, body);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) _logger.LogCritical("Failed to Send Email to @{email}", email);
        }
    }
}
