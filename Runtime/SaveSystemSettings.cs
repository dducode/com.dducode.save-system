using System.Text;
using SaveSystem.Internal.Templates;
using SaveSystem.Security;
using UnityEngine;

namespace SaveSystem {

    public class SaveSystemSettings : ScriptableObject {

        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnSceneLoad | SaveEvents.OnExit;
        public LogLevel enabledLogs = LogLevel.Warning | LogLevel.Error;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 15;

        [Tooltip(Tooltips.IsParallel)]
        public bool isParallel;

        [Tooltip(Tooltips.DataPath)]
        public string dataPath = "default_data_file.data";

        public string playerTag = "Player";

        public bool encryption;
        public EncryptionSettings encryptionSettings;

        public bool authentication;
        public AuthenticationSettings authenticationSettings;


        public override string ToString () {
            var result = new StringBuilder();

            AppendCommonSettings(result);
            AppendEncryptionSettings(result);
            AppendAuthSettings(result);

            return result.ToString();
        }


        private void AppendCommonSettings (StringBuilder result) {
            result.Append($"\nEnabled Save Events: {enabledSaveEvents}");
            result.Append($"\nEnabled Logs: {enabledLogs}");
            if (enabledSaveEvents.HasFlag(SaveEvents.AutoSave))
                result.Append($"\nSave Period: {savePeriod} sec");
            result.Append($"\nParallel saving: {(isParallel ? "Enable" : "Disable")}");
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