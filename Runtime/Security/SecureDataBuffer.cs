using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Security;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage.Security {

    public record SecureDataBuffer : DataBuffer {

        private readonly IEncryptor m_encryptor;


        public SecureDataBuffer () {
            m_encryptor = new AesEncryptor(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                new KeyGenerationParams {
                    hashAlgorithm = HashAlgorithmName.SHA1,
                    keyLength = AESKeyLength._128Bit,
                    iterations = 5
                }
            );
        }


        internal SecureDataBuffer (SaveReader reader) : base(reader) {
            m_encryptor = new AesEncryptor(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                new KeyGenerationParams {
                    hashAlgorithm = HashAlgorithmName.SHA1,
                    keyLength = AESKeyLength._128Bit,
                    iterations = 5
                }
            );

            foreach (string key in commonBuffer.Keys.ToArray())
                commonBuffer[key] = m_encryptor.Encrypt(commonBuffer[key]);
        }


        public override void Write<TValue> (string key, TValue value) {
            base.Write(key, value);
            commonBuffer[key] = m_encryptor.Encrypt(commonBuffer[key]);
        }


        public override TValue Read<TValue> (string key, TValue defaultValue = default) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.TryGetValue(key, out byte[] value)
                ? MemoryMarshal.Read<TValue>(m_encryptor.Decrypt(value))
                : defaultValue;
        }


        public override void Write<TArray> (string key, TArray[] array) {
            base.Write(key, array);
            commonBuffer[key] = m_encryptor.Encrypt(commonBuffer[key]);
        }


        public override TArray[] ReadArray<TArray> (string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (commonBuffer.TryGetValue(key, out byte[] value)) {
                (byte[] length, byte[] data) split = m_encryptor.Decrypt(value).Split(sizeof(int));
                var array = new TArray[MemoryMarshal.Read<int>(split.length)];
                Span<byte> span = MemoryMarshal.AsBytes((Span<TArray>)array);
                for (var i = 0; i < span.Length; i++)
                    span[i] = split.data[i];
                return array;
            }
            else {
                return Array.Empty<TArray>();
            }
        }


        public override void Write (string key, string value) {
            base.Write(key, value);
            commonBuffer[key] = m_encryptor.Encrypt(commonBuffer[key]);
        }


        public override string ReadString (string key, string defaultValue = null) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return commonBuffer.TryGetValue(key, out byte[] value)
                ? Encoding.Default.GetString(m_encryptor.Decrypt(value))
                : defaultValue;
        }


        internal override void WriteData (SaveWriter writer) {
            writer.Write(commonBuffer.Count);

            foreach (string key in commonBuffer.Keys) {
                writer.Write(Encoding.UTF8.GetBytes(key));
                writer.Write(m_encryptor.Decrypt(commonBuffer[key]));
            }

            HasChanges = false;
        }

    }

}