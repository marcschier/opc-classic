// 
// 
// 

namespace System {

    using SharpCifs.Util;
    using System.IO;

    /// <summary>
    /// Utils
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Helper to convert buffer into hex
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string HexString(byte[] buffer, int start, int length) {
            using (var writer = new StringWriter()) {
                Hexdump.ToHexdump(writer, buffer, start, length);
                return writer.ToString();
            }
        }
    }
}