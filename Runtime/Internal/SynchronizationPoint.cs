using System;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace SaveSystem.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private Func<CancellationToken, UniTask<HandlingResult>> m_scheduledTask;


        internal void ScheduleTask (Func<CancellationToken, UniTask<HandlingResult>> task) {
            m_scheduledTask ??= task;
        }


        internal async UniTask<HandlingResult> ExecuteScheduledTask (CancellationToken token) {
            if (m_scheduledTask == null)
                return HandlingResult.Canceled;

            try {
                return await ExecuteTask(async () => await m_scheduledTask(token));
            }
            finally {
                m_scheduledTask = null;
            }
        }


        internal async UniTask<HandlingResult> ExecuteTask (Func<UniTask<HandlingResult>> task) {
            if (IsPerformed)
                return HandlingResult.Error;

            IsPerformed = true;

            try {
                return await task();
            }
            finally {
                IsPerformed = false;
            }
        }

    }

}