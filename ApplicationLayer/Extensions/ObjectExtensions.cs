using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Mesawer.ApplicationLayer.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Projects an object to the respective target by calling the static mapping method and send the optional parameters
    /// </summary>
    public static TTarget ProjectTo<TTarget, TSource>(this TSource source, params object[] optionalParameters)
        where TSource : class
    {
        if (source is null) return default;

        var method = typeof(TTarget).GetProjectToMethod(typeof(TSource), optionalParameters.Length);

        return method.InvokeProjectToMethod<TTarget>(source, optionalParameters);
    }

    /// <summary>
    /// Converts the passed in object to an enumerable of tuples consisting of the Name of the attribute and its value.
    /// </summary>
    private static IEnumerable<(string Name, object Value)> ToKeyValueTuples(this object obj)
        =>
            from prop in obj.GetType().GetProperties()
            let getter = prop.GetGetMethod(false)
            where getter != null
            let val = getter.Invoke(obj, null)
            select (prop.Name, val);


    /// <summary>
    /// Converts the value of a specified object into a JSON string and return
    /// byte representation of the string.
    /// </summary>
    public static byte[] ToBytes(this object obj) => Encoding.Default.GetBytes(obj.ToJson());

    /// <summary>
    /// Converts the value of a specified object into a JSON string and return
    /// byte representation of the string.
    /// </summary>
    public static byte[] ToBytes(this object obj, JsonSerializerOptions options)
        => Encoding.Default.GetBytes(obj.ToJson(options));

    /// <summary>
    /// Converts the value of a specified type into a JSON string.
    /// </summary>
    /// <param name="obj">The value to convert.</param>
    /// <param name="camelCase">Serialized property name style.</param>
    /// <returns>The JSON string representation of the value.</returns>
    public static string ToJson(this object obj, bool camelCase = true)
        => JsonSerializer.Serialize(
            obj,
            camelCase
                ? new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }
                : null);

    /// <summary>
    /// Converts the value of a specified type into a JSON string.
    /// </summary>
    /// <param name="obj">The value to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    /// <returns>The JSON string representation of the value.</returns>
    public static string ToJson(this object obj, JsonSerializerOptions options)
        => JsonSerializer.Serialize(obj, options);

    /// <summary>
    /// Used to simplify and beautify casting an object to a type.
    /// </summary>
    /// <typeparam name="T">Type to be cast-ed</typeparam>
    /// <param name="obj">Object to cast</param>
    /// <returns>Cast-ed object</returns>
    public static T As<T>(this object obj) => (T) obj;

    /// <summary>
    /// Check if an item is in a list.
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <param name="list">List of items</param>
    /// <typeparam name="T">Type of the items</typeparam>
    public static bool In<T>(this T item, params T[] list) => list.Contains(item);
}
