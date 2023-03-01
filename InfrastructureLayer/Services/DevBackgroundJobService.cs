using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mesawer.InfrastructureLayer.Services;

public class DevBackgroundJobService : IBackgroundJobService
{
    private readonly IServiceProvider _serviceProvider;

    public DevBackgroundJobService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public string Enqueue(Expression<Action> method)
    {
        method.Compile().Invoke();

        return string.Empty;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var service = _serviceProvider.GetRequiredService<T>();

        methodCall.Compile().Invoke(service).Wait();

        return string.Empty;
    }

    public string Enqueue(Expression<Func<Task>> method)
    {
        method.Compile().Invoke().Wait();

        return string.Empty;
    }

    public string Schedule(Expression<Action> method, TimeSpan delay)
    {
        Task.Run(() =>
        {
            Task.Delay(delay);

            method.Compile().Invoke();
        });

        return string.Empty;
    }

    public string Schedule(Expression<Func<Task>> method, TimeSpan delay) => throw new NotImplementedException();
    public string Schedule(Expression<Action> method, DateTimeOffset enqueueAt) => throw new NotImplementedException();

    public string Schedule(Expression<Func<Task>> method, DateTimeOffset enqueueAt)
        => throw new NotImplementedException();

    public bool Delete(string jobId) => throw new NotImplementedException();
}
