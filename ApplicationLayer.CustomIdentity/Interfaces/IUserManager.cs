using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.CustomIdentity.Models;
using Mesawer.DomainLayer.CustomIdentity.Entities;
using Mesawer.DomainLayer.CustomIdentity.Enums;

namespace Mesawer.ApplicationLayer.CustomIdentity.Interfaces
{
    public interface IUserManager<in TUser, TRole>
        where TUser : ApplicationUser where TRole : Enum
    {
        Task<IList<TRole>> GetRolesAsync(TUser user);

        Task<IdentityResult> AddAccountAsync(TUser user, AccountInfo<TRole> accountInfo);

        Task<IList<SocialLoginInfo>> GetLoginsAsync(TUser user);
        Task<IdentityResult> AddLoginAsync(TUser user, SocialUser socialUser);
        Task<IdentityResult> RemoveLoginAsync(TUser user, LoginProvider loginProvider);

        Task<IdentityResult> SetPasswordAsync(TUser user, string password);
        Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword);
        Task<IdentityResult> RemovePasswordAsync(TUser user);

        Task<IdentityResult> CreateAsync(TUser user);
        Task<IdentityResult> CreateAsync(TUser user, string password);
        Task<IdentityResult> DeleteAsync(TUser user);

        Task<IdentityResult> ConfirmEmailAsync(TUser user, string token);
        Task<IdentityResult> ChangeEmailAsync(TUser user, string newEmail, string token);

        Task<IdentityResult> SetPhoneNumberAsync(TUser user, string phoneNumber, string token);
        Task<IdentityResult> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token);
    }
}
