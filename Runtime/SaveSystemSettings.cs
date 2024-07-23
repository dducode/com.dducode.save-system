using System;
using System.Text;
using SaveSystemPackage.Internal.Templates;
using SaveSystemPackage.Security;
using UnityEngine;

namespace SaveSystemPackage {

    public class SaveSystemSettings : ScriptableObject, IDisposable {

        public bool automaticInitialize = true;
        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave;
        public LogLevel enabledLogs = LogLevel.Warning | LogLevel.Error;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 15;

        [Tooltip(Tooltips.DataPath)]
        public string dataPath;

        public string playerTag = "Player";

        public bool encryption = true;
        public EncryptionSettings encryptionSettings;

        public bool authentication = true;
        public AuthenticationSettings authenticationSettings;


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
            result.Append($"\nEncryption: {(encryption ? "Enable" : "Disable")}");
            if (encryption)
                result.Append($"\nEncryption Settings: {{{encryptionSettings}}}");
        }


        private void AppendAuthSettings (StringBuilder result) {
            result.Append($"\nAuthentication: {(authentication ? "Enable" : "Disable")}");
            if (authentication)
                result.Append($"\nAuthentication settings: {authenticationSettings}");
        }

    }

}