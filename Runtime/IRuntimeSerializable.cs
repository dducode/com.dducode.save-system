using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;

namespace SaveSystem {

    /// <summary>
    /// Implement this for simplifed handling of your objects.
    /// </summary>
    public interface IRuntimeSerializable {

        /// <summary>
        /// Writes some data to <see cref="SaveWriter"/>
        /// </summary>
        public void Serialize (SaveWriter writer);


        /// <summary>
        /// Gets <see cref="SaveReader"/> and read data from this
        /// </summary>
        public void Deserialize (SaveReader reader);

    }



    /// <summary>
    /// Implement this for simplifed handling of your objects asynchronously.
    /// </summary>
    public interface IAsyncRuntimeSerializable {

        /// <summary>
        /// Writes some data to <see cref="SaveWriter"/>
        /// </summary>
        public UniTask Serialize (SaveWriter writer, CancellationToken token);


        /// <summary>
        /// Gets <see cref="SaveReader"/> and read data from this
        /// </summary>
        public UniTask Deserialize (SaveReader reader, CancellationToken token);

    }

}