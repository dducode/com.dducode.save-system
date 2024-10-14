using System.Diagnostics.CodeAnalysis;

namespace SaveSystemPackage.Security {

    public interface IEncryptor {

        /// <summary>
        /// Encrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data to be encrypted </param>
        /// <returns> Encrypted data </returns>
        public byte[] Encrypt ([NotNull] byte[] data);


        /// <summary>
        /// Decrypts any data from a byte array
        /// </summary>
        /// <param name="data"> Data containing encrypted data </param>
        /// <returns> Decrypted data </returns>
        public byte[] Decrypt ([NotNull] byte[] data);

    }

}