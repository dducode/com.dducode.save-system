using System;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Compressing;
using SaveSystemPackage.Security;

namespace SaveSystemPackage {

    public sealed class SerializationSettings {

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
            m_encrypt = settings.encrypt;
            m_compressFiles = settings.compressFiles;

            if (m_encrypt)
                SetupCryptographer(settings.encryptionSettings);
            if (m_compressFiles)
                SetupFileCompressor(settings.compressionSettings);
        }


        private SerializationSettings (SerializationSettings settings) {
            Encrypt = settings.Encrypt;
            CompressFiles = settings.CompressFiles;
        }


        internal SerializationSettings Clone () {
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