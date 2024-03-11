using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace SaveSystem.Internal {

    public class CancelableOperationsHandler {

        public static async UniTask<HandlingResult> Execute (
            Func<UniTask<HandlingResult>> task, string sender, string onCancelMessage,
            Object context = null, CancellationToken token = default
        ) {
            try {
                token.ThrowIfCancellationRequested();
                return await task();
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(sender, onCancelMessage, context);
                return HandlingResult.Canceled;
            }
        }

    }

}