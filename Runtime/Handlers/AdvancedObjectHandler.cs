using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler"/> only for the IPersistentObjectAsync
    /// </summary>
    public class AdvancedObjectHandler : AbstractHandler<AdvancedObjectHandler> {

        private readonly string m_filePath;
        private readonly IPersistentObjectAsync[] m_objects;


        internal AdvancedObjectHandler (string filePath, IPersistentObjectAsync[] objects) {
            m_filePath = filePath;
            m_objects = objects;
        }


        /// <inheritdoc cref="ObjectHandler.SaveAsync"/>
        public async UniTask<HandlingResult> SaveAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(m_filePath);

            HandlingResult result = await InternalHandling.Advanced.TrySaveObjectsAsync(
                m_objects, unityWriter, savingProgress, token
            );

            return result;
        }


        /// <inheritdoc cref="ObjectHandler.LoadAsync"/>
        public async UniTask<HandlingResult> LoadAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersProvider.GetReader(m_filePath);

            if (unityReader is null) {
                return HandlingResult.FileNotExists;
            }
            else {
                HandlingResult result = await InternalHandling.Advanced.TryLoadObjectsAsync(
                    m_objects, unityReader, loadingProgress, token
                );

                return result;
            }
        }

    }

}