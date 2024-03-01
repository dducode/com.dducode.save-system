using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem.BinaryHandlers;

namespace SaveSystem {

    /// <summary>
    /// Implement this for simplifed handling of your objects.
    /// </summary>
    public interface IRuntimeSerializable {

        /// <summary>
        /// Writes some data to <see cref="BinaryHandlers.BinaryWriter"/>
        /// </summary>
        public void Serialize (BinaryWriter writer);


        /// <summary>
        /// Gets <see cref="BinaryReader"/> and read data from this
        /// </summary>
        public void Deserialize (BinaryReader reader);

    }



    /// <summary>
    /// Implement this for simplifed handling of your objects asynchronously.
    /// </summary>
    public interface IAsyncRuntimeSerializable {

        /// <summary>
        /// Writes some data to <see cref="BinaryHandlers.BinaryWriter"/>
        /// </summary>
        public UniTask Serialize (BinaryWriter writer, CancellationToken token);


        /// <summary>
        /// Gets <see cref="BinaryReader"/> and read data from this
        /// </summary>
        public UniTask Deserialize (BinaryReader reader, CancellationToken token);

    }

}