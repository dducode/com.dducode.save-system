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

                    if (settings.encryptionSettings.useCustomCryptographer)
                        return;

                    if (Cryptographer == null)
                        Cryptographer = new Cryptographer(settings.encryptionSettings);
                    else
                        Cryptographer.SetSettings(settings.encryptionSettings);
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
                m_compressFiles = value;

                if (m_compressFiles) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();

                    if (settings.compressionSettings.useCustomCompressor)
                        return;

                    if (FileCompressor == null)
                        FileCompressor = new FileCompressor(settings.compressionSettings);
                    else
                        FileCompressor.SetSettings(settings.compressionSettings);
                }
            }
        }


        [NotNull]
        public FileCompressor FileCompressor {
            get => m_fileCompressor;
            set => m_fileCompressor = value ?? throw new ArgumentNullException(nameof(FileCompressor));
        }


        private bool m_encrypt;
        private Cryptographer m_cryptographer;
        private bool m_compressFiles;
        private FileCompressor m_fileCompressor;

    }

}