using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace SaveSystem.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private readonly Queue<Func<CancellationToken, UniTask<HandlingResult>>> m_queue = new();


        internal void ScheduleTask (Func<CancellationToken, UniTask<HandlingResult>> task, bool isPriority = false) {
            if (isPriority || m_queue.Count == 0)
                m_queue.Enqueue(task);
        }


        internal async void ExecuteScheduledTask (CancellationToken token) {
            if (m_queue.Count == 0 || IsPerformed)
                return;

            try {
                await ExecuteTask(async () => await m_queue.Dequeue().Invoke(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning("Scheduled task was canceled");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal async UniTask<HandlingResult> ExecuteTask (Func<UniTask<HandlingResult>> task) {
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