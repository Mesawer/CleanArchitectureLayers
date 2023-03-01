using System;
using JetBrains.Annotations;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class DateTimeExtensions
{
    /// <summary>
    /// Adds the TimeSpan into the DateTime to create a DateTimeOffset
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, TimeSpan? timeSpan = null)
    {
        var offset = new DateTimeOffset(dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second,
            dateTime.Millisecond,
            timeSpan ?? TimeSpan.Zero);

        return timeSpan is not null ? offset.AddHours(((TimeSpan) timeSpan!).TotalHours) : offset;
    }

    /// <summary>
    /// Checks whether the original datetime comes before the other datetime or not.
    /// </summary>
    public static bool IsOlderThan(this DateTimeOffset original, DateTimeOffset other) => original < other;

    /// <summary>
    /// Checks whether the original datetime/with comes before the other datetime or not.
    /// </summary>
    public static bool IsOlderOrEqual(this DateTimeOffset original, DateTimeOffset other) => original <= other;

    /// <summary>
    /// Checks whether the original datetime comes after the other datetime or not.
    /// </summary>
    public static bool IsNewerThan(this DateTimeOffset original, DateTimeOffset other) => original > other;

    /// <summary>
    /// Checks whether the original datetime comes after/with the other datetime or not.
    /// </summary>
    public static bool IsNewerOrEqual(this DateTimeOffset original, DateTimeOffset other) => original >= other;
}
