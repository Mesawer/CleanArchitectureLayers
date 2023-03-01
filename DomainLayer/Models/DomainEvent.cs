using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Mesawer.DomainLayer.Models;

[PublicAPI]
public interface IHasDomainEvent
{
    public List<DomainEvent> DomainEvents { get; }
}

[PublicAPI]
public abstract class DomainEvent
{
    protected DomainEvent() => DateOccurred = DateTimeOffset.UtcNow;

    public bool IsPublished { get; set; }
    public DateTimeOffset DateOccurred { get; protected set; }

    protected void Deserialize(string serialized)
    {
        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized);

        if (dictionary is null) return;

        var properties = GetType().GetProperties().ToList();

        foreach (var property in properties)
        {
            var sValue = dictionary[property.Name];

            if (sValue is null) continue;

            var value = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(sValue), property.PropertyType);

            var setMethod = GetBackingField(property);

            setMethod?.SetValue(this, value);
        }
    }

    public static DomainEvent GetDomainEvent(Type type, string serialized)
    {
        if (Activator.CreateInstance(type, true) is not DomainEvent instance)
            throw new InvalidOperationException($"Can't create an instance of type {type.Name}");

        instance.Deserialize(serialized);

        return instance;
    }

    private static FieldInfo GetBackingField(PropertyInfo propertyInfo)
        => propertyInfo.DeclaringType!
            .GetField($"<{propertyInfo.Name}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
}
