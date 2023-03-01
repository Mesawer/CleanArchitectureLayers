using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Interfaces;

[PublicAPI]
public interface IRecurringJobService
{
    void AddOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    void RemoveIfExists(string recurringJobId);
}
