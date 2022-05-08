using System;
using System.Collections.Generic;
using Mesawer.ApplicationLayer;
using Microsoft.Extensions.Hosting;

namespace Mesawer.PresentationLayer;

public class PresentationOptions
{
    private readonly string _environmentName;

    public PresentationOptions()
        => _environmentName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariableName);

    public string ApiTitle { get; set; }
    public string Version { get; set; } = "1.0";

    public string CorsPolicy { get; set; } = "CorsPolicy";

    public IEnumerable<KeyValuePair<string, string>> CorsOrigins { get; set; }

    internal bool IsDevelopment => _environmentName == Environments.Development;
    internal bool IsStaging => _environmentName == Environments.Staging;
    internal bool IsProduction => _environmentName == Environments.Production;
}
