using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    /// <summary>
    /// Object Handler can help you to saving/loading <see cref="IPersistentObject">persistent objects</see>.
    /// Also you can set parameters for this as a chain of methods
    /// </summary>
    public sealed class ObjectHandler : AbstractHandler<ObjectHandler> {

        private readonly string m_localFilePath;
        private readonly IPersistentObject[] m_objects;


        internal ObjectHandler (string localFilePath, IPersistentObject[] objects) {
            m_localFilePath = localFilePath;
            m_objects = objects;
        }


        /// <summary>
        /// Call it to start objects saving
        /// </summary>
        public void Save () {
            if (token.IsCancellationRequested)
                return;

            using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(m_localFilePath);

            foreach (IPersistentObject obj in m_objects)
                obj.Save(unityWriter);

            unityWriter.WriteBufferToFile();
        }


        /// <summary>
        /// Call it to start objects loading
        /// </summary>
        public HandlingResult Load () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersProvider.GetReader(m_localFilePath);

            if (unityReader.ReadFileDataToBuffer()) {
                foreach (IPersistentObject obj in m_objects)
                    obj.Load(unityReader);

                return HandlingResult.Success;
            }

            return HandlingResult.FileNotExists;
        }


        /// <summary>
        /// Call it to start async objects saving
        /// </summary>
        public async UniTask<HandlingResult> SaveAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriter(m_localFilePath);

            HandlingResult result = await InternalHandling.TrySaveObjectsAsync(
                m_objects, asyncMode, unityWriter, savingProgress, token
            );

            if (result == HandlingResult.Success)
                await unityWriter.WriteBufferToFileAsync();

            return result;
        }


        /// <summary>
        /// Call it to start async objects loading
        /// </summary>
        public async UniTask<HandlingResult> LoadAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = UnityHandlersProvider.GetReader(m_localFilePath);

            if (await unityReader.ReadFileDataToBufferAsync()) {
                HandlingResult result = await InternalHandling.TryLoadObjectsAsync(
                    m_objects, asyncMode, unityReader, loadingProgress, token
                );

                return result;
            }

            return HandlingResult.FileNotExists;
        }

    }

}