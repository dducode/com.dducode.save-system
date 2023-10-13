using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace SaveSystem.Internal {

    internal static class ParallelLoop {

        /// <summary>
        /// Parallel version of foreach loop
        /// </summary>
        internal static async UniTask ForEachAsync<T> (
            [NotNull] IEnumerable<T> source, [NotNull] Func<T, UniTask> body
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var tasks = new List<UniTask>();

            foreach (T obj in source)
                tasks.Add(body(obj));

            await UniTask.WhenAll(tasks);
        }


        /// <summary>
        /// Parallel version of foreach loop
        /// </summary>
        internal static async UniTask ForEachAsync<T> (
            [NotNull] IEnumerable<T> source,
            [NotNull] Func<T, UniTask> body,
            IProgress<float> progress,
            CancellationToken token = default
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var tasks = new List<UniTask>();

            foreach (T obj in source)
                tasks.Add(body(obj));

            for (var i = 0; i < tasks.Count; i++) {
                await UniTask.WhenAny(tasks);
                if (token.IsCancellationRequested)
                    return;
                progress?.Report((float)i / tasks.Count);
            }
        }

    }

}