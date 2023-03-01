using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Interfaces;

[PublicAPI]
public interface IDomainEventService
{
    Task Publish(string fullName, string serializedDomainEvent);
}
