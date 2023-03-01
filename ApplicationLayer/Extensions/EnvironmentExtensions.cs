using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class EnvironmentExtensions
{
    public static T GetEnvironmentVariable<T>(string key, T defaultValue = default)
    {
        var value = Environment.GetEnvironmentVariable(key);

        return value is not null ? JsonConvert.DeserializeObject<T>(value) : defaultValue;
    }

    public static void SetEnvironmentVariable<T>(string key, T value)
        => Environment.SetEnvironmentVariable(key, JsonConvert.SerializeObject(value));
}
