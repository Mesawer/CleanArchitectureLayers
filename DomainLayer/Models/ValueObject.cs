using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Mesawer.DomainLayer.Models;

// Learn more: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
[PublicAPI]
public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null) return false;

        return left?.Equals(right) != false;
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right) => !(EqualOperator(left, right));

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType()) return false;

        var other = (ValueObject) obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
}
