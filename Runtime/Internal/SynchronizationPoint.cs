using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace SaveSystem.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private readonly Queue<Func<CancellationToken, UniTask>> m_queue = new();


        internal void ScheduleTask (Func<CancellationToken, UniTask> task, bool priority = false) {
            if (priority || m_queue.Count == 0)
                m_queue.Enqueue(task);
        }


        internal async void ExecuteScheduledTask (CancellationToken token) {
            if (m_queue.Count == 0)
                return;

            try {
                await ExecuteTask(async () => await m_queue.Dequeue().Invoke(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SynchronizationPoint), "Scheduled task was canceled");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal async UniTask ExecuteTask (Func<UniTask> task) {
            await WaitCurrentExecution();

            IsPerformed = true;

            try {
                await task();
            }
            finally {
                IsPerformed = false;
            }
        }


        internal async UniTask<TResult> ExecuteTask<TResult> (Func<UniTask<TResult>> task) {
            await WaitCurrentExecution();

            IsPerformed = true;

            try {
                return await task();
            }
            finally {
                IsPerformed = false;
            }
        }


        private async UniTask WaitCurrentExecution (CancellationToken token = default) {
            while (IsPerformed)
                await UniTask.Yield(token);
        }

    }

}