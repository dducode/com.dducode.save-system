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
        public AESKeyLength keyLength = AESKeyLength._128Bit;


        public override string ToString () {
            return $"\nEnabled Save Events: {enabledSaveEvents}" +
                   $"\nEnabled Logs: {enabledLogs}" +
                   $"\nSave Period: {savePeriod} sec" +
                   $"\nParallel saving: {(isParallel ? "Enable" : "Disable")}" +
                   $"\nData Path: {dataPath}" +
                   $"\nPlayer Tag: {playerTag}" +
                   $"\nEncryption: {(encryption ? "Enable" : "Disable")}" +
                   $"\nAES Key Length: {keyLength}";
        }

    }

}