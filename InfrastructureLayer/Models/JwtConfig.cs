namespace Mesawer.InfrastructureLayer.Models
{
    public record JwtConfig
    {
        /// <summary>
        /// Gets or Sets the secret key that identities our applications.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or Sets the issuer for the security key.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or Sets the expiration period (in days) for the generated security key.
        /// </summary>
        public int ExpirationPeriod { get; set; } = 1;
    }
}
