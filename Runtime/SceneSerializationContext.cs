using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public sealed class SceneSerializationContext : SerializationContext {

        public async Task Reload (CancellationToken token = default) {
            try {
                token.ThrowIfCancellationRequested();
                await OnReloadInvoke();
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.Log(Name, "Data reload canceled");
            }
        }


        internal async Task Save (SaveType saveType, CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                await OnSaveInvoke(saveType);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.Log(Name, "Data saving canceled");
            }
        }

    }

}