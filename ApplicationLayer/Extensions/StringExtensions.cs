using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Mesawer.ApplicationLayer.Extensions;

[PublicAPI]
public static class StringExtensions
{
    public static bool IsEmptyConfig(this string str) => string.IsNullOrEmpty(str) || str == "EMPTY";

    /// <summary>
    /// Deserializes the JSON to the specified .NET type
    /// with default value in case of invalid conversion.
    /// </summary>
    public static T DeserializeSafely<T>(this string value, T defaultValue = default)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type
    /// with default value in case of invalid conversion.
    /// </summary>
    public static object DeserializeSafely(this string value)
    {
        try
        {
            return JsonConvert.DeserializeObject(value);
        }
        catch
        {
            return value;
        }
    }

    /// <summary>
    /// Generates a username from the user's email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static string GenerateUsername(this string email)
    {
        var key = string.Join(string.Empty, email.Split('@').First().ToCharArray().Take(50).ToArray());

        return $"{key}.{Guid.NewGuid().ToString().Split('-').First()[..4]}";
    }

    /// <summary>
    /// Convert a string to get its possible perma key.
    /// </summary>
    public static string ToPermaKey(this string str, bool useKey = false)
    {
        var vanilla = str.ToLower().ToArray();

        var valid   = vanilla.Where(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or ' ').ToArray();
        var invalid = vanilla.Except(valid).ToArray();

        var output = new string(valid).Trim().Replace(" ", "-");
        var key    = Guid.NewGuid().ToString().Split('-').First();

        if (useKey) return $"{output}-{key}";

        return invalid.Any() ? $"{output}-{key[..4]}" : output;
    }

    /// <summary>
    /// Strips any single quotes or double quotes from the beginning and end of a string.
    /// </summary>
    public static string StripQuotes(this string s) => Regex.Replace(s, "^\\s*['\"]+|['\"]+\\s*$", string.Empty);

    /// <summary>
    /// Returns a string that represents the current object, using CultureInfo.InvariantCulture where possible.
    /// Dates are represented in IS0 8601.
    /// inspired by: http://stackoverflow.com/a/19570016/62600
    /// </summary>
    public static string ToInvariantString(this object obj)
        => obj == null                ? null :
            obj is DateTime dt        ? dt.ToString("o", CultureInfo.InvariantCulture) :
            obj is DateTimeOffset dto ? dto.ToString("o", CultureInfo.InvariantCulture) :
            obj is IConvertible c     ? c.ToString(CultureInfo.InvariantCulture) :
            obj is IFormattable f     ? f.ToString(null, CultureInfo.InvariantCulture) :
                                        obj.ToString();

    internal static bool OrdinalEquals(this string s, string value, bool ignoreCase = false)
        => s != null && s.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static bool OrdinalContains(this string s, string value, bool ignoreCase = false)
        => s != null &&
           s.Contains(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static bool OrdinalStartsWith(this string s, string value, bool ignoreCase = false)
        => s != null && s.StartsWith(value,
            ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static bool OrdinalEndsWith(this string s, string value, bool ignoreCase = false)
        => s != null && s.EndsWith(value,
            ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    /// <summary>
    /// Splits at the first occurrence of the given separator.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="separator">The separator to split on.</param>
    /// <returns>Array of at most 2 strings. (1 if separator is not found.)</returns>
    public static string[] SplitOnFirstOccurrence(this string s, string separator)
    {
        // Needed because full PCL profile doesn't support Split(char[], int) (#119)
        if (string.IsNullOrEmpty(s)) return new[] { s };

        var i = s.IndexOf(separator, StringComparison.Ordinal);
        return (i == -1) ? new[] { s } : new[] { s[..i], s[(i + separator.Length)..] };
    }

    /// <summary>
    /// Returns null if the string is null or empty or white spaces
    /// otherwise return a trim-ed string.
    /// </summary>
    public static string NullableTrim(this string input) => input.IsNullOrWhiteSpace() ? null : input.Trim();

    /// <summary>
    /// Indicates whether this string is null or an System.String.Empty string.
    /// </summary>
    public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// indicates whether this string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string NormalizeLineEndings(this string str)
        => str.Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", Environment.NewLine);

    /// <summary>
    /// Replace any white space characters [\r\n\t\f\v ] with one white space.
    /// </summary>
    public static string OneSpace(this string input) => Regex.Replace(input, @"\s+", " ");

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string EnsureStartsWith(
        this string str,
        char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (str.StartsWith(c.ToString(), comparisonType)) return str;

        return c + str;
    }

    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string EnsureEndsWith(
        this string str,
        char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (str.EndsWith(c.ToString(), comparisonType)) return str;
        return str + c;
    }

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string[] SplitToLines(this string str) => str.Split(Environment.NewLine);

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string[] SplitToLines(this string str, StringSplitOptions options)
        => str.Split(Environment.NewLine, options);

    /// <summary>
    /// Converts string to enum value.
    /// </summary>
    /// <typeparam name="T">Type of enum</typeparam>
    /// <param name="value">String value to convert</param>
    /// <param name="ignoreCase">Ignore case</param>
    /// <returns>Returns enum object</returns>
    public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct
        => (T) Enum.Parse(typeof(T), value, ignoreCase);

    /// <summary>
    /// Gets a substring of a string from beginning of the string.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
    public static string Left(this string str, int len)
    {
        if (str.Length < len)
            throw new ArgumentException(
                "len argument can not be bigger than given string's length!");

        return str[..len];
    }

    /// <summary>
    /// Gets a substring of a string from end of the string.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
    public static string Right(this string str, int len)
    {
        if (str.Length < len)
            throw new ArgumentException(
                "len argument can not be bigger than given string's length!");

        return str.Substring(str.Length - len, len);
    }

    /// <summary>
    /// Replace first occurrence of <paramref name="search"/> in the string <paramref name="str"/>
    /// with <paramref name="replace"/>.
    /// </summary>
    public static string ReplaceFirst(
        this string str,
        string search,
        string replace,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        var pos = str.IndexOf(search, comparisonType);

        return pos < 0
            ? str
            : str[..pos] + replace + str[(pos + search.Length)..];
    }

    /// <summary>
    /// Removes first occurrence of the given postfixes from end of the given string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="postFixes">one or more postfix.</param>
    /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
    public static string RemovePostfix(this string str, params string[] postFixes)
        => str.RemovePostfix(StringComparison.Ordinal, postFixes);

    /// <summary>
    /// Removes first occurrence of the given postfixes from end of the given string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="comparisonType">String comparison type</param>
    /// <param name="postfixes">one or more postfix.</param>
    /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
    public static string RemovePostfix(this string str, StringComparison comparisonType, params string[] postfixes)
    {
        if (str.IsNullOrEmpty()) return null;

        if (postfixes.IsNullOrEmpty()) return str;

        foreach (var postFix in postfixes)
            if (str.EndsWith(postFix, comparisonType))
                return str.Left(str.Length - postFix.Length);

        return str;
    }

    /// <summary>
    /// Removes first occurrence of the given prefixes from beginning of the given string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="prefixes">one or more prefix.</param>
    /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
    public static string RemovePrefix(this string str, params string[] prefixes)
        => str.RemovePrefix(StringComparison.Ordinal, prefixes);

    /// <summary>
    /// Removes first occurrence of the given prefixes from beginning of the given string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="comparisonType">String comparison type</param>
    /// <param name="prefixes">one or more prefix.</param>
    /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
    public static string RemovePrefix(this string str, StringComparison comparisonType, params string[] prefixes)
    {
        if (str.IsNullOrEmpty()) return null;

        if (prefixes.IsNullOrEmpty()) return str;

        foreach (var preFix in prefixes)
            if (str.StartsWith(preFix, comparisonType))
                return str.Right(str.Length - preFix.Length);

        return str;
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
    public static string Truncate(this string str, int maxLength)
    {
        if (str is null) return null;

        return str.Length <= maxLength
            ? str
            : str.Left(maxLength);
    }

    /// <summary>
    /// Gets a substring of a string from Ending of the string if it exceeds maximum length.
    /// </summary>
    public static string TruncateFromBeginning(this string str, int maxLength)
    {
        if (str is null) return null;

        return str.Length <= maxLength
            ? str
            : str.Right(maxLength);
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// It adds a "..." postfix to end of the string if it's truncated.
    /// Returning string can not be longer than maxLength.
    /// </summary>
    public static string TruncateWithPostfix(this string str, int maxLength, string postfix = "...")
    {
        if (str == null) return null;

        if (str == string.Empty || maxLength == 0) return string.Empty;

        if (str.Length <= maxLength) return str;

        if (maxLength <= postfix.Length) return postfix.Left(maxLength);

        return str.Left(maxLength - postfix.Length) + postfix;
    }

    /// <summary>
    /// Converts given string to a byte array using <see cref="Encoding.UTF8"/> encoding.
    /// </summary>
    public static byte[] GetBytes(this string str) => str.GetBytes(Encoding.UTF8);

    /// <summary>
    /// Converts given string to a byte array using the given <paramref name="encoding"/>
    /// </summary>
    public static byte[] GetBytes(this string str, Encoding encoding) => encoding.GetBytes(str);

    /// <summary>
    /// Gets index of nth occurrence of a char in a string.
    /// </summary>
    /// <param name="str">source string to be searched</param>
    /// <param name="c">Char to search in <see cref="str"/></param>
    /// <param name="n">Count of the occurrence</param>
    public static int IndexOf(this string str, char c, int n)
    {
        var count = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] != c) continue;
            if (++count == n) return i;
        }

        return -1;
    }

    /// <summary>
    /// Replace a new string applying on it <see cref="string.Replace(string, string)"/>
    /// using <paramref name="replaces"/>.
    /// </summary>
    public static string Replace(this string str, IEnumerable<(string oldValue, string newValue)> replaces)
    {
        var output = str[..];

        foreach (var (oldValue, newValue) in replaces) output = output.Replace(oldValue, newValue);

        return output;
    }

    /// <summary>
    /// Replace a new string applying on it <see cref="string.Replace(char, char)"/>
    /// using <paramref name="replaces"/>.
    /// </summary>
    public static string Replace(this string str, IEnumerable<(char oldValue, char newValue)> replaces)
    {
        var output = str[..];

        foreach (var (oldValue, newValue) in replaces) output = output.Replace(oldValue, newValue);

        return output;
    }

    /// <summary>
    /// Convert any digit in the string to the equivalent english digit.
    /// <para>"١٢٨" to "128"</para>
    /// <para>"١,٢٨" to "1,28"</para>
    /// <para>"١.٢٨" to "1.28"</para>
    /// <para>"This ١٢٨" to "This 128"</para>
    /// </summary>
    public static string ToEnglishDigits(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var sb = new StringBuilder();

        foreach (var c in input)
            if (char.IsDigit(c))
                sb.Append(char.GetNumericValue(c).ToString(CultureInfo.InvariantCulture));
            else
                sb.Append(c);

        return sb.ToString();
    }

    /// <summary>
    /// Convert any digit in the string to the equivalent english digit.
    /// <para>"١٢٨" to "128"</para>
    /// <para>"١,٢٨" to "1,28"</para>
    /// <para>"١.٢٨" to "1.28"</para>
    /// <para>"This ١٢٨" to "This 128"</para>
    /// </summary>
    public static string ToEnglishDigits(this IEnumerable<char> input)
    {
        var sb = new StringBuilder();

        foreach (var c in input)
            if (char.IsDigit(c))
                sb.Append(char.GetNumericValue(c).ToString(CultureInfo.InvariantCulture));
            else
                sb.Append(c);

        return sb.ToString();
    }

    /// <summary>
    /// Remove accents (diacritics) from the string.
    /// <para>"crème brûlée" to "creme-brulee"</para>
    /// <para>"بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ" to "بسم الله الرحمن الرحيم"</para>
    /// </summary>
    /// <remarks>
    /// Remarks:
    /// <para>* Normalize to FormD splits accented letters in letters+accents.</para>
    /// <para>* Remove those accents (and other non-spacing characters).</para>
    /// <para>* Return a new string from the remaining chars.</para>
    /// </remarks>
    public static string RemoveAccents(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var cs =
            from ch in input.Normalize(NormalizationForm.FormD)
            let category = CharUnicodeInfo.GetUnicodeCategory(ch)
            where category != UnicodeCategory.NonSpacingMark
            select ch;

        var sb = new StringBuilder();

        foreach (var c in cs) sb.Append(c);

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Compares two strings, character by character, and returns the
    /// first position where the two strings differ from one another.
    /// </summary>
    /// <param name="s1">
    /// The first string to compare
    /// </param>
    /// <param name="s2">
    /// The second string to compare
    /// </param>
    /// <returns>
    /// The first position where the two strings differ.
    /// </returns>
    public static int StringDifference(this string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var len  = len1 < len2 ? len1 : len2;

        for (var i = 0; i < len; i++)
            if (s1[i] != s2[i])
                return i;

        return len;
    }
}
