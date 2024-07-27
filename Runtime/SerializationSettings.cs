using System;
using System.Diagnostics.CodeAnalysis;
using SaveSystemPackage.Security;
using SaveSystemPackage.Verification;

namespace SaveSystemPackage {

    public sealed class SerializationSettings {

        public bool Encrypt {
            get => m_encrypt;
            set {
                m_encrypt = value;

                if (m_encrypt) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();

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

        public bool VerifyChecksum {
            get => m_verifyChecksum;
            set {
                m_verifyChecksum = value;

                if (m_verifyChecksum) {
                    using SaveSystemSettings settings = SaveSystemSettings.Load();

                    if (VerificationManager == null)
                        VerificationManager = new VerificationManager(settings.verificationSettings);
                    else
                        VerificationManager.SetSettings(settings.verificationSettings);
                }
            }
        }

        [NotNull]
        public VerificationManager VerificationManager {
            get => m_verificationManager;
            set => m_verificationManager = value ?? throw new ArgumentNullException(nameof(VerificationManager));
        }

        private bool m_encrypt;
        private Cryptographer m_cryptographer;
        private bool m_verifyChecksum;
        private VerificationManager m_verificationManager;

    }

}