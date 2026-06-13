// 
// 
// 

namespace System;

/// <summary>
/// Utils
/// </summary>
public static class Utils
{
    /// <summary>
    /// Helper to convert buffer into hex
    /// </summary>
    /// <param name="buffer">Buffer containing the bytes or fields being processed.</param>
    /// <param name="start">Zero-based start index of the substring or range.</param>
    /// <param name="length">Number of bytes or elements to process.</param>
    /// <returns>The hex string text value.</returns>
    public static string HexString(byte[] buffer, int start, int length) => Convert.ToHexString(buffer, start, length);
}
