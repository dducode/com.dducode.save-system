using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystemPackage.Internal {

    internal sealed class SynchronizationPoint {

        internal bool IsPerformed { get; private set; }

        private readonly Queue<Func<CancellationToken, Task>> m_queue = new();


        internal void ScheduleTask (Func<CancellationToken, Task> task, bool priority = false) {
            if (priority || m_queue.Count == 0)
                m_queue.Enqueue(task);
        }


        internal async void ExecuteScheduledTask (CancellationToken token) {
            try {
                await WaitCurrentExecution();

                if (m_queue.Count == 0)
                    return;

                await ExecuteTask(async () => await m_queue.Dequeue().Invoke(token));
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.LogWarning(nameof(SynchronizationPoint), "Scheduled task was canceled");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }


        internal async Task ExecuteTask (Func<Task> task) {
            await WaitCurrentExecution();

            IsPerformed = true;

            try {
                await task();
            }
            finally {
                IsPerformed = false;
            }
        }


        internal void Clear () {
            m_queue.Clear();
        }


        private async Task WaitCurrentExecution () {
            while (IsPerformed)
                await Task.Yield();
        }

    }

}