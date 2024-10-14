using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Security {

    public interface IEncryptor {

        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data to be encrypted </param>
        /// <returns> Encrypted data </returns>
        public byte[] Encrypt ([NotNull] byte[] data);


        /// <summary>
        /// Encrypts any data from a stream
        /// </summary>
        /// <param name="stream"> Stream to be encrypted </param>
        /// <param name="token"></param>
        public Task Encrypt ([NotNull] Stream stream, CancellationToken token = default);


        /// <summary>
        /// Decrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data containing encrypted data </param>
        /// <returns> Decrypted data </returns>
        public byte[] Decrypt ([NotNull] byte[] data);


        /// <summary>
        /// Decrypts any data from a stream
        /// </summary>
        /// <param name="stream"> Stream containing encrypted data </param>
        /// <param name="token"></param>
        public Task Decrypt ([NotNull] Stream stream, CancellationToken token = default);

    }

}