using System;
using System.Threading;
#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.HandlingResult>;

#else
using TaskAlias = System.Threading.Tasks.Task;
using TaskBool = System.Threading.Tasks.Task<bool>;
using TaskResult = System.Threading.Tasks.Task<SaveSystem.HandlingResult>;
#endif


namespace SaveSystem.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private Func<CancellationToken, TaskAlias> m_task;


        internal void SetTask (Func<CancellationToken, TaskAlias> task) {
            m_task ??= task;
        }


        internal async TaskAlias ExecuteTask (CancellationToken token) {
            if (m_task == null)
                return;

            await ExecuteTask(m_task, token);
            m_task = null;
        }


        internal async TaskAlias ExecuteTask (Func<CancellationToken, TaskAlias> task, CancellationToken token) {
            if (IsPerformed)
                return;

            IsPerformed = true;
            await task(token);
            IsPerformed = false;
        }


        internal async TaskResult ExecuteTask (Func<CancellationToken, TaskResult> task, CancellationToken token) {
            if (IsPerformed)
                return HandlingResult.InternalError;

            IsPerformed = true;

            try {
                return await task(token);
            }
            finally {
                IsPerformed = false;
            }
        }

    }

}