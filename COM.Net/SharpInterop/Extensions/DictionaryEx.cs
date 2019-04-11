


namespace System.Collections.Generic {
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
        public static object GetAndRemove(this IDictionary<object, object> dictionary, object key) {
            if (dictionary.ContainsKey(key)) {
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
        public static object GetAndRemoveAt(this IList<object> list, int index) {
            if (index < list.Count) {
                var value = list[index];
                list.Remove(index);
                return value;
            }
            return null;
        }
    }
}
