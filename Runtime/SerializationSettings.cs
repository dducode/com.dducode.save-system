using System;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Security;
using SaveSystemPackage.Serialization;

namespace SaveSystemPackage {

    public sealed class SerializationSettings : ICloneable<SerializationSettings> {

        public ISerializer Serializer {
            get => m_serializer;
            set => m_serializer = value ?? throw new ArgumentNullException(nameof(Serializer));
        }

        public IKeyProvider KeyProvider {
            get => m_keyProvider;
            set => m_keyProvider = value ?? throw new ArgumentNullException(nameof(KeyProvider));
        }

        public bool Encrypt {
            get => m_encrypt;
            set {
                if (value == m_encrypt)
                    return;

                m_encrypt = value;

                if (m_encrypt) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();
                    SetupCryptographer(settings.encryptionSettings);
                    m_serializer = new EncryptionSerializer(m_serializer, m_cryptographer);
                }
            }
        }

        [NotNull]
        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set => m_cryptographer = value ?? throw new ArgumentNullException(nameof(Cryptographer));
        }

        public bool CompressFiles {
            get => m_compressFiles;
            set {
                if (value == m_compressFiles)
                    return;

                m_compressFiles = value;

                if (m_compressFiles) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();
                    SetupFileCompressor(settings.compressionSettings);
                }
            }
        }

        [NotNull]
        public FileCompressor FileCompressor {
            get => m_fileCompressor;
            set => m_fileCompressor = value ?? throw new ArgumentNullException(nameof(FileCompressor));
        }

        private ISerializer m_serializer;
        private IKeyProvider m_keyProvider;

        private bool m_encrypt;
        private Cryptographer m_cryptographer;
        private bool m_compressFiles;
        private FileCompressor m_fileCompressor;


        public static implicit operator SerializationSettings (SaveSystemSettings settings) {
            return new SerializationSettings(settings);
        }


        private SerializationSettings (SaveSystemSettings settings) {
            m_serializer = SelectSerializer(settings.serializerType);
            m_keyProvider = new KeyStore();
            m_compressFiles = settings.compressFiles;
            if (m_compressFiles)
                SetupFileCompressor(settings.compressionSettings);

            m_encrypt = settings.encrypt;

            if (m_encrypt) {
                SetupCryptographer(settings.encryptionSettings);
                m_serializer = new EncryptionSerializer(m_serializer, m_cryptographer);
            }
        }


        private SerializationSettings (SerializationSettings settings) {
            m_compressFiles = settings.CompressFiles;
            if (m_compressFiles)
                m_fileCompressor = settings.FileCompressor.Clone();

            m_encrypt = settings.Encrypt;
            if (m_encrypt)
                m_cryptographer = settings.Cryptographer.Clone();
        }


        public SerializationSettings Clone () {
            return new SerializationSettings(this);
        }


        private void SetupCryptographer (EncryptionSettings settings) {
            if (settings.useCustomCryptographer) {
                Cryptographer = settings.reference;
                return;
            }

            if (Cryptographer == null)
                Cryptographer = new Cryptographer(settings);
            else
                Cryptographer.SetSettings(settings);
        }


        private void SetupFileCompressor (CompressionSettings settings) {
            if (settings.useCustomCompressor) {
                FileCompressor = settings.reference;
                return;
            }

            if (FileCompressor == null)
                FileCompressor = new FileCompressor(settings);
            else
                FileCompressor.SetSettings(settings);
        }


        private ISerializer SelectSerializer (SerializerType serializerType) {
            switch (serializerType) {
                case SerializerType.BinarySerializer:
                    return new BinarySerializer();
                case SerializerType.JsonSerializer:
                    return new JsonSerializer();
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, null);
            }
        }

    }

}