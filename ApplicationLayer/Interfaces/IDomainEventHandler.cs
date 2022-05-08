using System.Threading;
using System.Threading.Tasks;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IDomainEventHandler<in TDomainEvent>
{
    Task Handle(TDomainEvent e, CancellationToken ct);
}
