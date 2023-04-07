using System.Collections.Generic;

namespace UnityEPL {

    public static class IListExtensions {
        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Returns a shuffled copy of the IList.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rng"></param>
        public static IList<T> Shuffle<T>(this IList<T> list) {
            var shuf = new List<T>(list);
            for (int i = shuf.Count - 1; i > 0; i--) {
                int j = InterfaceManager.rnd.Value.Next(i + 1);
                T tmp = shuf[i];
                shuf[i] = shuf[j];
                shuf[j] = tmp;
            }

            return shuf;
        }

        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Returns a shuffled copy of the List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rng"></param>
        public static List<T> Shuffle<T>(this List<T> list) {
            var shuf = new List<T>(list);
            for (int i = shuf.Count - 1; i > 0; i--) {
                int j = InterfaceManager.rnd.Value.Next(i + 1);
                T tmp = shuf[i];
                shuf[i] = shuf[j];
                shuf[j] = tmp;
            }

            return shuf;
        }
    }

    public static class CollectionExtensions {
        /// <summary>
        /// Allows List constructor to take a items or a list of items that gets expanded
        /// https://stackoverflow.com/a/63374611
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="itemsToAdd"></param>
        public static void Add<T>(this ICollection<T> collection, IEnumerable<T> itemsToAdd) {
            foreach (var item in itemsToAdd) {
                collection.Add(item);
            }
        }
    }

}