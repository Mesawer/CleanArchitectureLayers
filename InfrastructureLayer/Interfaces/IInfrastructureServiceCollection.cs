using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.InfrastructureLayer.Interfaces;

public interface IInfrastructureServiceCollection
{
    public IServiceCollection Services { get; set; }
    public InfrastructureOptions Options { get; set; }
}
