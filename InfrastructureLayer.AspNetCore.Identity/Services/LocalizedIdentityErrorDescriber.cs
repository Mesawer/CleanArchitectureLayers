using Mesawer.InfrastructureLayer.Resources.Identity;
using Microsoft.AspNetCore.Identity;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Services
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
            => new()
            {
                Code        = nameof(DuplicateEmail),
                Description = string.Format(IdentityRes.DuplicateEmail, email)
            };

        public override IdentityError DuplicateUserName(string userName)
            => new()
            {
                Code        = nameof(DuplicateUserName),
                Description = string.Format(IdentityRes.DuplicateUserName, userName)
            };

        public override IdentityError InvalidEmail(string email)
            => new()
            {
                Code        = nameof(InvalidEmail),
                Description = string.Format(IdentityRes.InvalidEmail, email)
            };

        public override IdentityError DuplicateRoleName(string role)
            => new()
            {
                Code        = nameof(DuplicateRoleName),
                Description = string.Format(IdentityRes.DuplicateRoleName, role)
            };

        public override IdentityError InvalidRoleName(string role)
            => new()
            {
                Code        = nameof(InvalidRoleName),
                Description = string.Format(IdentityRes.InvalidRoleName, role)
            };

        public override IdentityError InvalidToken()
            => new()
            {
                Code        = nameof(InvalidToken),
                Description = IdentityRes.InvalidToken
            };

        public override IdentityError InvalidUserName(string userName)
            => new()
            {
                Code        = nameof(InvalidUserName),
                Description = string.Format(IdentityRes.InvalidUserName, userName)
            };

        public override IdentityError LoginAlreadyAssociated()
            => new()
            {
                Code        = nameof(LoginAlreadyAssociated),
                Description = IdentityRes.LoginAlreadyAssociated
            };

        public override IdentityError PasswordMismatch()
            => new()
            {
                Code        = nameof(PasswordMismatch),
                Description = IdentityRes.PasswordMismatch
            };

        public override IdentityError PasswordRequiresDigit()
            => new()
            {
                Code        = nameof(PasswordRequiresDigit),
                Description = IdentityRes.PasswordRequiresDigit
            };

        public override IdentityError PasswordRequiresLower()
            => new()
            {
                Code        = nameof(PasswordRequiresLower),
                Description = IdentityRes.PasswordRequiresLower
            };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new()
            {
                Code        = nameof(PasswordRequiresNonAlphanumeric),
                Description = IdentityRes.PasswordRequiresNonAlphanumeric
            };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new()
            {
                Code        = nameof(PasswordRequiresUniqueChars),
                Description = string.Format(IdentityRes.PasswordRequiresUniqueChars, uniqueChars)
            };

        public override IdentityError PasswordRequiresUpper()
            => new()
            {
                Code        = nameof(PasswordRequiresUpper),
                Description = IdentityRes.PasswordRequiresUpper
            };

        public override IdentityError PasswordTooShort(int length)
            => new()
            {
                Code        = nameof(PasswordTooShort),
                Description = string.Format(IdentityRes.PasswordTooShort, length)
            };

        public override IdentityError UserAlreadyHasPassword()
            => new()
            {
                Code        = nameof(UserAlreadyHasPassword),
                Description = IdentityRes.UserAlreadyHasPassword
            };

        public override IdentityError UserAlreadyInRole(string role)
            => new()
            {
                Code        = nameof(UserAlreadyInRole),
                Description = string.Format(IdentityRes.UserAlreadyInRole, role)
            };

        public override IdentityError UserNotInRole(string role)
            => new()
            {
                Code        = nameof(UserNotInRole),
                Description = string.Format(IdentityRes.UserNotInRole, role)
            };

        public override IdentityError UserLockoutNotEnabled()
            => new()
            {
                Code        = nameof(UserLockoutNotEnabled),
                Description = IdentityRes.UserLockoutNotEnabled
            };

        public override IdentityError RecoveryCodeRedemptionFailed()
            => new()
            {
                Code        = nameof(RecoveryCodeRedemptionFailed),
                Description = IdentityRes.RecoveryCodeRedemptionFailed
            };

        public override IdentityError ConcurrencyFailure()
            => new()
            {
                Code        = nameof(ConcurrencyFailure),
                Description = IdentityRes.ConcurrencyFailure
            };

        public override IdentityError DefaultError()
            => new()
            {
                Code        = nameof(DefaultError),
                Description = IdentityRes.DefaultIdentityError
            };
    }
}
