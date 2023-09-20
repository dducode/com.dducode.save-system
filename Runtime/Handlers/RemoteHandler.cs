using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Handlers {

    public class RemoteHandler : AbstractHandler<RemoteHandler> {

        private readonly string m_url;
        private readonly IPersistentObject[] m_objects;


        internal RemoteHandler (string url, IPersistentObject[] objects) {
            m_url = url;
            m_objects = objects;
        }


        /// <summary>
        /// Call it to start remote objects saving
        /// </summary>
        public async UniTask<HandlingResult> SaveAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            await using UnityWriter unityWriter = UnityHandlersProvider.GetWriterRemote();

            HandlingResult result = await InternalHandling.TrySaveObjectsAsync(
                m_objects, asyncMode, unityWriter, savingProgress, token
            );

            if (result == HandlingResult.Success) {
                bool sendingSucceeded = await Storage.SendDataToRemote(m_url, unityWriter.GetMemoryData());
                result = sendingSucceeded ? HandlingResult.Success : HandlingResult.NetworkError;
            }

            return result;
        }


        /// <summary>
        /// Call it to start remote objects loading
        /// </summary>
        public async UniTask<HandlingResult> LoadAsync () {
            if (token.IsCancellationRequested)
                return HandlingResult.CanceledOperation;

            using UnityReader unityReader = await UnityHandlersProvider.GetReaderRemote(m_url);

            if (unityReader is null) {
                return HandlingResult.NetworkError;
            }
            else {
                HandlingResult result = await InternalHandling.TryLoadObjectsAsync(
                    m_objects, asyncMode, unityReader, loadingProgress, token
                );

                return result;
            }
        }

    }

}