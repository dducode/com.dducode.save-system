#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Text;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Templates;
using SaveSystemPackage.Security;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SaveSystemPackage {

    public class SaveSystemSettings : ScriptableObject, IDisposable {

        public bool automaticInitialize = true;
        public LogLevel enabledLogs = LogLevel.Warning | LogLevel.Error;
        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 5;

        [Tooltip(Tooltips.DataPath)]
        public string dataPath;

    #if ENABLE_BOTH_SYSTEMS
        public UsedInputSystem usedInputSystem;
    #endif

    #if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode quickSaveKey = KeyCode.S;
        public KeyCode screenCaptureKey = KeyCode.Print;
    #endif

    #if ENABLE_INPUT_SYSTEM
        public InputActionReference quickSaveAction;
        public InputActionReference screenCaptureAction;
    #endif

        public string playerTag = "Player";

        public bool encrypt = true;
        public EncryptionSettings encryptionSettings;

        public bool verifyChecksum = true;
        public VerificationSettings verificationSettings;


        private void OnEnable () {
            if (string.IsNullOrEmpty(dataPath))
                dataPath = $"{Application.productName.ToPathFormat()}.data";
        }


        public override string ToString () {
            var result = new StringBuilder();

            AppendCommonSettings(result);
            AppendEncryptionSettings(result);
            AppendAuthSettings(result);

            return result.ToString();
        }


        public void Dispose () {
            Resources.UnloadAsset(this);
        }


        private void AppendCommonSettings (StringBuilder result) {
            result.Append($"\nEnabled Save Events: {enabledSaveEvents}");
            result.Append($"\nEnabled Logs: {enabledLogs}");
            if (enabledSaveEvents.HasFlag(SaveEvents.PeriodicSave))
                result.Append($"\nSave Period: {savePeriod} sec");
            result.Append($"\nData Path: {dataPath}");
            result.Append($"\nPlayer Tag: {playerTag}");
        }


        private void AppendEncryptionSettings (StringBuilder result) {
            result.Append($"\nEncryption: {(encrypt ? "Enable" : "Disable")}");
            if (encrypt)
                result.Append($"\nEncryption Settings: {{{encryptionSettings}}}");
        }


        private void AppendAuthSettings (StringBuilder result) {
            result.Append($"\nAuthentication: {(verifyChecksum ? "Enable" : "Disable")}");
            if (verifyChecksum)
                result.Append($"\nAuthentication settings: {verificationSettings}");
        }

    }

}