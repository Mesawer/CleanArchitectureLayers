using System;
using Mesawer.ApplicationLayer;
using Mesawer.InfrastructureLayer.Models;
using Mesawer.InfrastructureLayer.Services;
using Microsoft.Extensions.Hosting;

namespace Mesawer.InfrastructureLayer
{
    public class InfrastructureOptions
    {
        private readonly string _environmentName;

        public InfrastructureOptions()
            => _environmentName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariableName);

        public string Version { get; set; } = "1.0";

        public string ConnectionString { get; set; } = "DefaultConnection";

        private SendGridConfig _sendGridConfig = new();

        /// <summary>
        /// Configures the SendGrid options.
        /// </summary>
        public SendGridConfig SendGridConfig
        {
            get => _sendGridConfig ?? new SendGridConfig();
            set => _sendGridConfig = value;
        }

        private IdentityOptions _identityOptions = new();

        /// <summary>
        /// Configures the identity options.
        /// </summary>
        public IdentityOptions IdentityOptions
        {
            get => _identityOptions ?? new IdentityOptions();
            set => _identityOptions = value;
        }

        private ApplicationInsightsConfig _applicationInsightsConfig = new();

        /// <summary>
        /// Configures Application Insights services into service collection.
        /// </summary>
        public ApplicationInsightsConfig ApplicationInsightsConfig
        {
            get => _applicationInsightsConfig ?? new ApplicationInsightsConfig();
            set => _applicationInsightsConfig = value;
        }

        public Type DomainEventService { get; set; } = typeof(DomainEventService);
        public Type EmailService { get; set; } = typeof(SendGridEmailSender);
        public Type StorageLocationService { get; set; } = typeof(LocalStorageLocationService);
        public Type StorageService { get; set; } = typeof(LocalStorageService);

        public bool IsDevelopment => _environmentName == Environments.Development;
        public bool IsStaging => _environmentName == Environments.Staging;
        public bool IsProduction => _environmentName == Environments.Production;
    }
}
