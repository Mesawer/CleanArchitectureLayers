using System;
using System.ComponentModel;
using System.Linq;

namespace Mesawer.ApplicationLayer.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Converts an enum to array of its values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] ToArray<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().ToArray();

    /// <summary>
    /// Get the description of the the enum declared by the description attribute.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum value)
    {
        if (value == null) return string.Empty;

        var attribute = value.GetAttribute<DescriptionAttribute>();

        return attribute == null ? value.ToString() : attribute.Description;
    }

    /// <summary>
    /// Get the instance of the attribute declared in an enum.
    /// </summary>
    private static T GetAttribute<T>(this Enum value) where T : Attribute
    {
        if (value == null) return null;

        var member = value.GetType().GetMember(value.ToString());

        var attributes = member[0].GetCustomAttributes(typeof(T), false);

        return (T) attributes[0];
    }
}
