﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public sealed class SceneSerializationScope : SerializationScope {

        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data reload canceled");
            }
        }


        internal async Task Save (SaveType saveType, CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await OnSaveInvoke(saveType);
            }
            catch (OperationCanceledException) {
                Logger.Log(Name, "Data saving canceled");
            }
        }

    }

}