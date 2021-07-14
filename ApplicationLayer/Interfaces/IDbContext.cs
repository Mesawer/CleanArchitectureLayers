using System.Threading;
using System.Threading.Tasks;
using Mesawer.DomainLayer.Entities;
using Mesawer.DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface IDbContext<TSession> where TSession : Session
    {
        public DbSet<TSession> Sessions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task TriggerDomainEventHandlers(params DomainEvent[] domainEvents);

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        void RollbackTransaction();
    }
}
