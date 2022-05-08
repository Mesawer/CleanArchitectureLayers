using System;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Serilog;

namespace Mesawer.InfrastructureLayer.Services;

public class LocalEmailSender : IEmailSender
{
    public Task SendAsync(
        string type,
        string name,
        string email,
        string subject,
        EmailTemplate template)
    {
        var fileName =
            $"{DateTime.Now:yy-MM-dd__HH-mm-ss-tt}-{Guid.NewGuid().ToString("N")[..4]}";

        using var emailLogger = new LoggerConfiguration()
            .WriteTo.File($"Logs/Emails/{fileName}.html")
            .CreateLogger();

        var htmlBody = string.Empty;

        emailLogger.Information(
            "Type: {Type} To: {To}; Subject: {Subject}; {Body}; {Text}",
            type,
            email,
            subject,
            htmlBody,
            template.Content);

        return Task.CompletedTask;
    }
}
