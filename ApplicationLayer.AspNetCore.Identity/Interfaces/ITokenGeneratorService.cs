using System.Threading.Tasks;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;

namespace Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;

public interface ITokenGeneratorService<in T> where T : ApplicationUser
{
    Task<string> GenerateEmailConfirmationTokenAsync(T user);
    Task<string> GenerateChangeEmailTokenAsync(T user, string newEmail);
    Task<string> GeneratePasswordResetTokenAsync(T user);
    Task<string> GeneratePhoneNumberConfirmationTokenAsync(T user);
    Task<string> GenerateChangePhoneNumberTokenAsync(T user, string phoneNumber);
}
