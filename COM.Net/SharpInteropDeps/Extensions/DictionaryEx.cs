


namespace System.Collections {
    using System;
    using System.Text;

    /// <summary>
    /// Extensions
    /// </summary>
    public static class CollectionsEx {

        /// <summary>
        /// Mimics java
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetAndRemove(this IDictionary dictionary, object key) {
            if (dictionary.Contains(key)) {
                var value = dictionary[key];
                dictionary.Remove(key);
                return value;
            }
            return null;
        }
        /// <summary>
        /// Mimics java
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object GetAndRemoveAt(this IList list, int index) {
            if (index < list.Count) {
                var value = list[index];
                list.Remove(index);
                return value;
            }
            return null;
        }
    }
}
