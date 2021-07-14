namespace Mesawer.InfrastructureLayer.Models
{
    public class SendGridConfig
    {
        /// <summary>
        /// Gets or Sets the SendGrid Api Key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or Sets the email from which all emails will be sent.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or Sets the name by which all emails will be sent
        /// </summary>
        public string FromName { get; set; }
    }
}
