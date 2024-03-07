using System.Text;
using SaveSystem.Cryptography;
using SaveSystem.Internal.Templates;
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
        public bool useCustomProviders;
        public string password;
        public string saltKey;
        public KeyGenerationParams keyGenerationParams = KeyGenerationParams.Default;


        public override string ToString () {
            var result = new StringBuilder();
            result.Append($"\nEnabled Save Events: {enabledSaveEvents}");
            result.Append($"\nEnabled Logs: {enabledLogs}");
            if (enabledSaveEvents.HasFlag(SaveEvents.AutoSave))
                result.Append($"\nSave Period: {savePeriod} sec");
            result.Append($"\nParallel saving: {(isParallel ? "Enable" : "Disable")}");
            result.Append($"\nData Path: {dataPath}");
            result.Append($"\nPlayer Tag: {playerTag}");
            result.Append($"\nEncryption: {(encryption ? "Enable" : "Disable")}");
            if (encryption)
                result.Append($"\nKey Generation Parameters: {{{keyGenerationParams}}}");
            return result.ToString();
        }

    }

}