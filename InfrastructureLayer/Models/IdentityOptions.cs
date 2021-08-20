using Mesawer.ApplicationLayer.Enums;

namespace Mesawer.InfrastructureLayer.Models
{
    public class IdentityOptions
    {
        /// <summary>
        /// Determines whether the user is only allowed to login once or not
        /// </summary>
        public bool RestrictSingleLogin { get; set; } = false;

        /// <summary>
        /// Determines whether to log users out on new login or not
        /// </summary>
        public bool LogoutOnNewLogin { get; set; } = false;

        /// <summary>
        /// Determines whether the user is allowed to login on the same device or not
        /// </summary>
        public bool RestrictSingleDevice { get; set; } = false;

        /// <summary>
        /// Determines whether the generated token will be numeric or not
        /// </summary>
        public bool NumericVerificationToken { get; set; } = false;

        public int NumericTokenLength { get; set; } = 6;

        /// <summary>
        /// The period after which the generated session will be expired (in hours)
        /// </summary>
        public int SessionExpirationPeriod { get; set; } = 24;

        /// <summary>
        /// Determines whether a confirmed email address is required to sign in
        /// </summary>
        public bool RequireConfirmedEmail { get; set; } = true;

        /// <summary>
        /// Determines whether a confirmed phone number is required to sign in
        /// </summary>
        public bool RequireConfirmedPhoneNumber { get; set; } = false;

        /// <summary>
        /// Determines whether the application requires unique emails for its users
        /// </summary>
        public bool RequireUniqueEmail { get; set; } = true;

        /// <summary>
        /// Determines whether the application will lockout the user after several failed attempts
        /// </summary>
        public bool LockoutOnFailure { get; set; } = true;

        /// <summary>
        /// The period after which the generated token will be expired (in minutes)
        /// </summary>
        public int TokenExpirationPeriod { get; set; } = 60;

        /// <summary>
        /// The period before which the user can't generate another token (in minutes)
        /// </summary>
        public int TokenResetPeriod { get; set; } = 5;

        public PhoneNumberCode[] AcceptedCodes { get; set; }

        private PasswordOptions _password = new();

        /// <summary>
        /// Gets or sets the PasswordOptions for the identity system.
        /// </summary>
        public PasswordOptions Password { get => _password ?? new PasswordOptions(); set => _password = value; }

        private JwtConfig _jwtConfig = new();

        /// <summary>
        /// Configures the JWT options.
        /// </summary>
        public JwtConfig JwtConfig { get => _jwtConfig ?? new JwtConfig(); set => _jwtConfig = value; }
    }
}
