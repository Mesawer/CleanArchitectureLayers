using JetBrains.Annotations;

namespace Mesawer.InfrastructureLayer;

[PublicAPI]
public record HangfireOptions
{
    public bool UseHangfire { get; set; }

    public bool UseInMemoryDatabase { get; set; }

    public string ConnectionString { get; set; }
}
