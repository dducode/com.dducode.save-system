﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace SaveSystemPackage.Internal {

    public class CancelableOperationsHandler {

        public static async UniTask Execute (
            Func<UniTask> task, string sender, string onCancelMessage,
            Object context = null, CancellationToken token = default
        ) {
            try {
                token.ThrowIfCancellationRequested();
                await task();
            }
            catch (OperationCanceledException) {
                Logger.LogWarning(sender, onCancelMessage, context);
            }
        }

    }

}