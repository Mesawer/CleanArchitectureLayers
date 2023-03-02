using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer;

[PublicAPI]
public static class Constants
{
    #region General

    public const string EnvironmentVariableName           = "DOTNET_ENVIRONMENT";
    public const string AspNetCoreEnvironmentVariableName = "ASPNETCORE_ENVIRONMENT";
    public const string SpaProxyBaseUrlName               = "SPA_BASE_URL";

    #endregion

    #region Validation

    public const int IdMaxLength   = 32;
    public const int MacMaxLength  = 17;
    public const int NameMaxLength = 32;
    public const int UriMaxLength  = 38;
    public const int MaxUrlLength  = 65519;

    #endregion

    public static string GetDirectory()
    {
        var location = Assembly.GetExecutingAssembly().Location;

        var lastIndex = location.LastIndexOf("/", StringComparison.Ordinal);

        if (lastIndex < 0) lastIndex = location.LastIndexOf("\\", StringComparison.Ordinal);

        return location.Remove(lastIndex);
    }
}
