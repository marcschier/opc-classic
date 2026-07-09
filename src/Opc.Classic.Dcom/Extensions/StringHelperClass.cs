// -------------------------------------------------------------------------------------------
//    Copyright © 2007 - 2014 Tangible Software Solutions Inc.
//    This class can be used by anyone provided that the copyright notice remains intact.
//
//    This class is used to convert some aspects of the Java String class.
// -------------------------------------------------------------------------------------------

namespace System;

/// <summary>
/// Provides compatibility helpers for Java-style string operations used by translated code.
/// </summary>
public static class StringHelperClass
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    // ----------------------------------------------------------------------------------
    //    This method replaces the Java String.substring method when 'start' is a
    //    method call or calculated value to ensure that 'start' is obtained just once.
    // ----------------------------------------------------------------------------------
    /// <summary>
    /// Returns a substring using Java-style start and end indexes while evaluating the indexes only once.
    /// </summary>
    /// <param name="self">String instance on which the helper method operates.</param>
    /// <param name="start">Zero-based start index of the substring or range.</param>
    /// <param name="end">Exclusive end index of the substring or range.</param>
    /// <returns>The substring special text value.</returns>
    public static string SubstringSpecial(this string self, int start, int end) => self.Substring(start, end - start);

    // ------------------------------------------------------------------------------
    //    This method is used to replace most calls to the Java String.split method.
    // ------------------------------------------------------------------------------
    /// <summary>
    /// Splits a string with a regular expression delimiter and optionally removes trailing empty results.
    /// </summary>
    /// <param name="self">String instance on which the helper method operates.</param>
    /// <param name="regexDelimiter">Regular expression delimiter used to split the string.</param>
    /// <param name="trimTrailingEmptyStrings">Value indicating whether trailing empty split results should be removed.</param>
    /// <returns>The sequence of split values produced by the operation.</returns>
    public static string[] Split(this string self, string regexDelimiter, bool trimTrailingEmptyStrings)
    {
        var splitArray = Text.RegularExpressions.Regex.Split(
            self,
            regexDelimiter,
            Text.RegularExpressions.RegexOptions.None,
            RegexTimeout);

        if (trimTrailingEmptyStrings)
        {
            if (splitArray.Length > 1)
            {
                for (var i = splitArray.Length; i > 0; i--)
                {
                    if (splitArray[i - 1].Length > 0)
                    {
                        if (i < splitArray.Length)
                        {
                            Array.Resize(ref splitArray, i);
                        }

                        break;
                    }
                }
            }
        }

        return splitArray;
    }

    /// <summary>
    /// Creates a string from UTF-8 or named-encoding bytes using Java-style helper semantics.
    /// </summary>
    /// <param name="bytes">Byte sequence containing the wire-format payload.</param>
    /// <returns>A new <see cref="string"/> instance built from <paramref name="bytes"/>.</returns>
    public static string NewString(byte[] bytes) => NewString(bytes, 0, bytes.Length);

    /// <summary>
    /// Creates a string from UTF-8 or named-encoding bytes using Java-style helper semantics.
    /// </summary>
    /// <param name="bytes">Byte sequence containing the wire-format payload.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <param name="count">Number of elements or bytes included in the operation.</param>
    /// <returns>A new <see cref="string"/> instance built from <paramref name="bytes"/>.</returns>
    public static string NewString(byte[] bytes, int index, int count) => Text.Encoding.UTF8.GetString((byte[])(object)bytes, index, count);

    /// <summary>
    /// Creates a string from UTF-8 or named-encoding bytes using Java-style helper semantics.
    /// </summary>
    /// <param name="bytes">Byte sequence containing the wire-format payload.</param>
    /// <param name="encoding">Text encoding name or encoding instance used for byte conversion.</param>
    /// <returns>A new <see cref="string"/> instance built from <paramref name="bytes"/>.</returns>
    public static string NewString(byte[] bytes, string encoding) => NewString(bytes, 0, bytes.Length, encoding);

    /// <summary>
    /// Creates a string from UTF-8 or named-encoding bytes using Java-style helper semantics.
    /// </summary>
    /// <param name="bytes">Byte sequence containing the wire-format payload.</param>
    /// <param name="index">Zero-based index at which the read or write operation begins.</param>
    /// <param name="count">Number of elements or bytes included in the operation.</param>
    /// <param name="encoding">Text encoding name or encoding instance used for byte conversion.</param>
    /// <returns>A new <see cref="string"/> instance built from <paramref name="bytes"/>.</returns>
    public static string NewString(byte[] bytes, int index, int count, string encoding) => Text.Encoding.GetEncoding(encoding).GetString((byte[])(object)bytes, index, count);

    /// <summary>
    /// Encodes a string into bytes using UTF-8 or a named text encoding.
    /// </summary>
    /// <param name="self">String instance on which the helper method operates.</param>
    /// <returns>The sequence of bytes values produced by the operation.</returns>
    public static byte[] GetBytes(this string self) => GetSBytesForEncoding(Text.Encoding.UTF8, self);

    /// <summary>
    /// Encodes a string into bytes using UTF-8 or a named text encoding.
    /// </summary>
    /// <param name="self">String instance on which the helper method operates.</param>
    /// <param name="encoding">Text encoding name or encoding instance used for byte conversion.</param>
    /// <returns>The sequence of bytes values produced by the operation.</returns>
    public static byte[] GetBytes(this string self, string encoding) => GetSBytesForEncoding(Text.Encoding.GetEncoding(encoding), self);

    /// <summary>
    /// Encodes a string into a byte array with the specified text encoding.
    /// </summary>
    /// <param name="encoding">Text encoding name or encoding instance used for byte conversion.</param>
    /// <param name="s">Text value used as the s.</param>
    /// <returns>The sequence of sbytes for encoding values produced by the operation.</returns>
    private static byte[] GetSBytesForEncoding(Text.Encoding encoding, string s)
    {
        var sbytes = new byte[encoding.GetByteCount(s)];
        encoding.GetBytes(s, 0, s.Length, (byte[])(object)sbytes, 0);
        return sbytes;
    }
}
