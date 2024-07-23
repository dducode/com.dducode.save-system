using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace SaveSystemPackage.Internal {

    internal static class ParallelLoop {

        /// <summary>
        /// Parallel version of foreach loop
        /// </summary>
        internal static async UniTask ForEachAsync<TBody> (
            [NotNull] IEnumerable<TBody> source, [NotNull] Func<TBody, UniTask> body
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var tasks = new List<UniTask>();
            Parallel.ForEach(source, obj => tasks.Add(body(obj)));
            await UniTask.WhenAll(tasks);
        }

    }

}