// -------------------------------------------------------------------------------------------
//    Copyright © 2007 - 2014 Tangible Software Solutions Inc.
//    This class can be used by anyone provided that the copyright notice remains intact.
//
//    This class is used to convert some aspects of the Java String class.
// -------------------------------------------------------------------------------------------
namespace System;

/// <summary>
///
/// </summary>
public static class StringHelperClass
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    // ----------------------------------------------------------------------------------
    //    This method replaces the Java String.substring method when 'start' is a
    //    method call or calculated value to ensure that 'start' is obtained just once.
    // ----------------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <param name="self"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string SubstringSpecial(this string self, int start, int end) => self.Substring(start, end - start);

    // ------------------------------------------------------------------------------
    //    This method is used to replace most calls to the Java String.split method.
    // ------------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <param name="self"></param>
    /// <param name="regexDelimiter"></param>
    /// <param name="trimTrailingEmptyStrings"></param>
    /// <returns></returns>
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
    ///
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string NewString(byte[] bytes) => NewString(bytes, 0, bytes.Length);

    /// <summary>
    ///
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string NewString(byte[] bytes, int index, int count) => Text.Encoding.UTF8.GetString((byte[])(object)bytes, index, count);

    /// <summary>
    ///
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string NewString(byte[] bytes, string encoding) => NewString(bytes, 0, bytes.Length, encoding);

    /// <summary>
    ///
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string NewString(byte[] bytes, int index, int count, string encoding) => Text.Encoding.GetEncoding(encoding).GetString((byte[])(object)bytes, index, count);

    /// <summary>
    ///
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static byte[] GetBytes(this string self) => GetSBytesForEncoding(Text.Encoding.UTF8, self);

    /// <summary>
    ///
    /// </summary>
    /// <param name="self"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static byte[] GetBytes(this string self, string encoding) => GetSBytesForEncoding(Text.Encoding.GetEncoding(encoding), self);

    /// <summary>
    ///
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    private static byte[] GetSBytesForEncoding(Text.Encoding encoding, string s)
    {
        var sbytes = new byte[encoding.GetByteCount(s)];
        encoding.GetBytes(s, 0, s.Length, (byte[])(object)sbytes, 0);
        return sbytes;
    }
}
