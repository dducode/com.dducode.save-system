using System;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Security;

namespace SaveSystemPackage {

    public sealed class SerializationSettings : ICloneable<SerializationSettings> {

        public bool Encrypt {
            get => m_encrypt;
            set {
                m_encrypt = value;

                if (m_encrypt) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();
                    SetupCryptographer(settings.encryptionSettings);
                }
            }
        }

        [NotNull]
        public Cryptographer Cryptographer {
            get => m_cryptographer;
            set => m_cryptographer = value ? value : throw new ArgumentNullException(nameof(Cryptographer));
        }

        public bool CompressFiles {
            get => m_compressFiles;
            set {
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
            set => m_fileCompressor = value ? value : throw new ArgumentNullException(nameof(FileCompressor));
        }

        private bool m_encrypt;
        private Cryptographer m_cryptographer;
        private bool m_compressFiles;
        private FileCompressor m_fileCompressor;


        public static implicit operator SerializationSettings (SaveSystemSettings settings) {
            return new SerializationSettings(settings);
        }


        private SerializationSettings (SaveSystemSettings settings) {
            m_compressFiles = settings.compressFiles;
            if (m_compressFiles)
                SetupFileCompressor(settings.compressionSettings);

            m_encrypt = settings.encrypt;
            if (m_encrypt)
                SetupCryptographer(settings.encryptionSettings);
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
                Cryptographer = settings.cryptographer;
                return;
            }

            if (Cryptographer == null)
                Cryptographer = Cryptographer.CreateInstance(settings);
            else
                Cryptographer.SetSettings(settings);
        }


        private void SetupFileCompressor (CompressionSettings settings) {
            if (settings.useCustomCompressor) {
                FileCompressor = settings.fileCompressor;
                return;
            }

            if (FileCompressor == null)
                FileCompressor = FileCompressor.CreateInstance(settings);
            else
                FileCompressor.SetSettings(settings);
        }

    }

}