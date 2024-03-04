using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace SaveSystem.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private Func<CancellationToken, UniTask<HandlingResult>> m_scheduledTask;


        internal void ScheduleTask (Func<CancellationToken, UniTask<HandlingResult>> task) {
            m_scheduledTask ??= task;
        }


        internal async void ExecuteScheduledTask (CancellationToken token) {
            if (m_scheduledTask == null || IsPerformed)
                return;

            try {
                await ExecuteTask(async () => await m_scheduledTask(token));
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(nameof(SynchronizationPoint), "Scheduled task was canceled");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                m_scheduledTask = null;
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