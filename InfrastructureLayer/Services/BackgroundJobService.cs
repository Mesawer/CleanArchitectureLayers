using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Mesawer.ApplicationLayer.Interfaces;

namespace Mesawer.InfrastructureLayer.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJob;

    public BackgroundJobService(IBackgroundJobClient backgroundJob) => _backgroundJob = backgroundJob;

    public string Enqueue(Expression<Action> method) => _backgroundJob.Enqueue(method);

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull
        => _backgroundJob.Enqueue(methodCall);

    public string Enqueue(Expression<Func<Task>> method) => _backgroundJob.Enqueue(method);

    public string Schedule(Expression<Action> method, TimeSpan delay) => _backgroundJob.Schedule(method, delay);

    public string Schedule(Expression<Func<Task>> method, TimeSpan delay) => _backgroundJob.Schedule(method, delay);

    public string Schedule(Expression<Action> method, DateTimeOffset enqueueAt)
        => _backgroundJob.Schedule(method, enqueueAt);

    public string Schedule(Expression<Func<Task>> method, DateTimeOffset enqueueAt)
        => _backgroundJob.Schedule(method, enqueueAt);

    public bool Delete(string jobId) => _backgroundJob.Delete(jobId);
}
