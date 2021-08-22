using Mesawer.InfrastructureLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.InfrastructureLayer.Services
{
    public class InfrastructureServiceCollection : IInfrastructureServiceCollection
    {
        public IServiceCollection Services { get; set; }
        public InfrastructureOptions Options { get; set; }
    }
}
