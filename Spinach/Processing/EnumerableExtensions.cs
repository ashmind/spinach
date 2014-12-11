using System;
using System.Collections.Generic;
using System.Linq;

namespace Spinach.Processing {
    internal static class EnumerableExtensions {
        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> selector) {
            var collected = new HashSet<T>(nodes);

            int found = 1;
            var lastLevel = (ICollection<T>)collected;
            while (found > 0) {
                var newLevel = lastLevel.SelectMany(selector).ToArray();
                found = 0;
                foreach (var node in newLevel) {
                    var added = collected.Add(node);
                    if (added)
                        found += 1;
                }

                lastLevel = newLevel;
            }

            return collected;
        }

        // TODO: Add to AshMind.Extensions
        public static ISet<T> AsSet<T>(this IEnumerable<T> values) {
            return (values as ISet<T>) ?? new HashSet<T>(values);
        }
    }
}
