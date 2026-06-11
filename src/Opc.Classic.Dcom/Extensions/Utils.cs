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
    /// <param name="buffer"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string HexString(byte[] buffer, int start, int length) => Convert.ToHexString(buffer, start, length);
}
