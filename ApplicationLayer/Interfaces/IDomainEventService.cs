using System.Threading;
using System.Threading.Tasks;
using Mesawer.DomainLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IDomainEventService
{
    Task Publish(DomainEvent e, CancellationToken ct = default);
}
