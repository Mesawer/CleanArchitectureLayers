using System;

namespace Mesawer.ApplicationLayer.Extensions
{
    public static class DateTimeExtensions
    {
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
}
