using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// It's same as <see cref="ObjectHandler"/> only for the IPersistentObjectAsync
    /// </summary>
    public class AdvancedObjectHandler : AbstractHandler<AdvancedObjectHandler> {

        private readonly string m_localFilePath;
        private readonly IPersistentObjectAsync[] m_objects;


        internal AdvancedObjectHandler (string localFilePath, IPersistentObjectAsync[] objects) {
            m_localFilePath = localFilePath;
            m_objects = objects;
        }


        /// <inheritdoc cref="ObjectHandler.SaveAsync"/>
        public async UniTask<HandlingResult> SaveAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(m_localFilePath);

            HandlingResult result = await InternalHandling.Advanced.TrySaveObjectsAsync(
                m_objects, unityWriter, savingProgress, token
            );

            if (result == HandlingResult.Success)
                await unityWriter.WriteBufferToFileAsync();

            return result;
        }


        /// <inheritdoc cref="ObjectHandler.LoadAsync"/>
        public async UniTask<HandlingResult> LoadAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersProvider.GetReader(m_localFilePath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                HandlingResult result = await InternalHandling.Advanced.TryLoadObjectsAsync(
                    m_objects, unityReader, loadingProgress, token
                );

                return result;
            }

            return HandlingResult.FileNotExists;
        }

    }

}