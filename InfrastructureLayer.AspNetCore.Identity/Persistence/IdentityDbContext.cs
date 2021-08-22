using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.AspNetCore.Identity.Interfaces;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.DomainLayer.AspNetCore.Identity.Entities;
using Mesawer.DomainLayer.Entities;
using Mesawer.DomainLayer.Models;
using Mesawer.InfrastructureLayer.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mesawer.InfrastructureLayer.AspNetCore.Identity.Persistence
{
    public class IdentityDbContext<TUser, TAccount, TSession>
        : IdentityDbContext<
                TUser,
                IdentityRole,
                string,
                IdentityUserClaim<string>,
                TAccount,
                IdentityUserLogin<string>,
                IdentityRoleClaim<string>,
                IdentityUserToken<string>>,
            IIdentityDbContext<TUser, TAccount, TSession>
        where TUser : ApplicationUser
        where TAccount : Account<TUser>
        where TSession : Session
    {
        private readonly IApplicationUserService _currentUserService;
        private readonly IDateTime               _dateTime;
        private readonly IDomainEventService     _domainEventService;
        private readonly IBackgroundJobService   _backgroundJob;
        private          IDbContextTransaction   _currentTransaction;

        public IdentityDbContext(
            DbContextOptions options,
            IApplicationUserService currentUserService,
            IDomainEventService domainEventService,
            IBackgroundJobService backgroundJob,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _domainEventService = domainEventService;
            _backgroundJob      = backgroundJob;
            _dateTime           = dateTime;
        }

        public DbSet<TAccount> Accounts => Set<TAccount>();
        public DbSet<TSession> Sessions => Set<TSession>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.Id;
                        entry.Entity.Created   = _dateTime.Now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.Id;
                        entry.Entity.LastModified   = _dateTime.Now;
                        break;

                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            var result = await base.SaveChangesAsync(cancellationToken);

            await TriggerDomainEventHandlers();

            return result;
        }

        public Task TriggerDomainEventHandlers(params DomainEvent[] domainEvents)
        {
            var tasks = GetDomainEventHandlersAsTasks(domainEvents);

            //if (tasks.Any()) _backgroundJob.Enqueue(() => FireAndForget(tasks));

            return FireAndForget(tasks);
        }

        public static Task FireAndForget(IEnumerable<Task> tasks) => Task.WhenAll(tasks);

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction is not null) return;

            _currentTransaction = await base.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted)
                .ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);

                var task = _currentTransaction?.CommitAsync();

                if (task is not null) await task;
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction is not null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction is not null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new AccountConfiguration<TUser, TAccount>());
            builder.ApplyConfiguration(new SessionConfiguration<TSession>());
            builder.ApplyConfiguration(new UserConfiguration<TUser, TSession>());

            builder.Entity<TUser>().ToTable("IdentityUsers");
            builder.Entity<TAccount>().ToTable("IdentityAccounts");
            builder.Entity<IdentityRole>().ToTable("IdentityRoles");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("IdentityRoleClaims");
            builder.Entity<IdentityUserClaim<string>>().ToTable("IdentityUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("IdentityUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("IdentityUserTokens");
        }

        private IEnumerable<Task> GetDomainEventHandlersAsTasks(params DomainEvent[] domainEvents)
        {
            var events = ChangeTracker
                .Entries<IHasDomainEvent>()
                .Select(e => e.Entity.DomainEvents)
                .SelectMany(e => e)
                .Where(domainEvent => !domainEvent.IsPublished)
                .ToList();

            if (domainEvents.Any()) events.AddRange(domainEvents);

            var tasks = new List<Task>();

            foreach (var entity in events)
            {
                entity.IsPublished = true;
                tasks.Add(_domainEventService.Publish(entity));
            }

            return tasks;
        }
    }
}
