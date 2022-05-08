using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mesawer.ApplicationLayer.Interfaces;

public interface IBackgroundJobService
{
    /// <summary>
    /// Creates a new fire-and-forget job based on a given method call expression.
    /// </summary>
    /// <param name="method">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="method"/> is <see langword="null"/>.
    /// </exception>
    string Enqueue(Expression<Action> method);

    /// <summary>
    /// Creates a new fire-and-forget job based on a given method call expression.
    /// </summary>
    /// <param name="method">Method call expression that will be marshalled to a server.</param>
    /// <returns>Unique identifier of a background job.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="method"/> is <see langword="null"/>.
    /// </exception>
    void Enqueue(Expression<Func<Task>> method);

    /// <summary>
    /// Creates a new background job based on a specified method
    /// call expression and schedules it to be enqueued after a given delay.
    /// </summary>
    /// <param name="method">Instance method call expression that will be marshalled to the Server.</param>
    /// <param name="delay">Delay, after which the job will be enqueued.</param>
    /// <returns>Unique identifier of the created job.</returns>
    void Schedule(Expression<Action> method, TimeSpan delay);

    /// <summary>
    /// Creates a new background job based on a specified method
    /// call expression and schedules it to be enqueued after a given delay.
    /// </summary>
    /// <param name="method">Instance method call expression that will be marshalled to the Server.</param>
    /// <param name="delay">Delay, after which the job will be enqueued.</param>
    /// <returns>Unique identifier of the created job.</returns>
    string Schedule(Expression<Func<Task>> method, TimeSpan delay);

    /// <summary>
    /// Creates a new background job based on a specified method call expression
    /// and schedules it to be enqueued at the given moment of time.
    /// </summary>
    /// <param name="method">Method call expression that will be marshalled to the Server.</param>
    /// <param name="enqueueAt">The moment of time at which the job will be enqueued.</param>
    /// <returns>Unique identifier of a created job.</returns>
    string Schedule(Expression<Action> method, DateTimeOffset enqueueAt);

    /// <summary>
    /// Creates a new background job based on a specified method call expression
    /// and schedules it to be enqueued at the given moment of time.
    /// </summary>
    /// <param name="method">Method call expression that will be marshalled to the Server.</param>
    /// <param name="enqueueAt">The moment of time at which the job will be enqueued.</param>
    /// <returns>Unique identifier of a created job.</returns>
    string Schedule(Expression<Func<Task>> method, DateTimeOffset enqueueAt);

    /// <summary>
    /// Changes state of a job with the specified <paramref name="jobId"/>
    /// to the deleted state.
    /// </summary>
    /// <param name="jobId">An identifier, that will be used to find a job.</param>
    /// <returns>True on a successful state transition, false otherwise.</returns>
    bool Delete(string jobId);
}
