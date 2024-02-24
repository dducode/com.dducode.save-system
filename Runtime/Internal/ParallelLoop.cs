using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;

#else
using System.Threading.Tasks;
using TaskAlias = System.Threading.Tasks.Task;
#endif

namespace SaveSystem.Internal {

    internal static class ParallelLoop {

        /// <summary>
        /// Parallel version of foreach loop
        /// </summary>
        internal static async TaskAlias ForEachAsync<TBody> (
            [NotNull] IEnumerable<TBody> source, [NotNull] Func<TBody, TaskAlias> body
        ) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var tasks = new List<TaskAlias>();
            Parallel.ForEach(source, obj => tasks.Add(body(obj)));
            await TaskAlias.WhenAll(tasks);
        }

    }

}