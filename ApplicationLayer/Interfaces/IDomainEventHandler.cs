using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Interfaces;

[PublicAPI]
public interface IDomainEventHandler<in TDomainEvent>
{
    Task Handle(TDomainEvent e, CancellationToken ct);
}
