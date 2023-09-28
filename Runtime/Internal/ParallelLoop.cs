using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace SaveSystem.Internal {

    internal static class ParallelLoop {

        /// <summary>
        /// Parallel version of foreach loop
        /// </summary>
        internal static async UniTask ForEachAsync<T> (IEnumerable<T> source, Func<T, UniTask> body) {
            var tasks = new List<UniTask>();

            foreach (T obj in source)
                tasks.Add(body(obj));

            await UniTask.WhenAll(tasks);
        }

    }

}