using System;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using UnityEngine;

namespace SaveSystem.Internal {

    internal static class Catcher {

        internal static async UniTask<HandlingResult> TryHandle (
            Func<UniTask> handling, string messageUponCancellation = null
        ) {
            try {
                await handling();
                return HandlingResult.Success;
            }
            catch (Exception ex) when (ex is OperationCanceledException) {
                if (!string.IsNullOrEmpty(messageUponCancellation))
                    Logger.LogWarning(messageUponCancellation);
                return HandlingResult.CanceledOperation;
            }
            catch (Exception ex) when (ex is UnityWebRequestException) {
                Debug.LogException(ex);
                return HandlingResult.NetworkError;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                return HandlingResult.UnknownError;
            }
        }

    }

}