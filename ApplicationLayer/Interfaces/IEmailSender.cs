using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string type, string name, string email, string subject, EmailTemplate template);
}
