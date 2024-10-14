#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Text;
using SaveSystemPackage.Internal.Templates;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SaveSystemPackage.Settings {

    public class SaveSystemSettings : ScriptableObject, IDisposable {

        public bool automaticInitialize = true;
        public LogLevel enabledLogs = LogLevel.Warning | LogLevel.Error;
        public SaveEvents enabledSaveEvents;
        public float logsFlushingTime = 5;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 5;

        public SerializerType serializerType;

    #if ENABLE_BOTH_SYSTEMS
        public UsedInputSystem usedInputSystem;
    #endif

    #if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode quickSaveKey = KeyCode.S;
    #endif

    #if ENABLE_INPUT_SYSTEM
        public InputActionReference quickSaveAction;
    #endif

        public string playerTag = "Player";

        public bool compress;
        public CompressionSettings compressionSettings;

        public bool encrypt;
        public EncryptionSettings encryptionSettings;

        public JsonSerializerSettings jsonSerializerSettings;


        internal static SaveSystemSettings Load () {
            return Resources.Load<SaveSystemSettings>($"Save System/{nameof(SaveSystemSettings)}");
        }


        internal static bool TryLoad (out SaveSystemSettings settings) {
            settings = Load();
            return settings != null;
        }


        public override string ToString () {
            var result = new StringBuilder();
            result.Append($"\nEnabled Save Events: {enabledSaveEvents}");
            result.Append($"\nEnabled Logs: {enabledLogs}");
            if (enabledSaveEvents.HasFlag(SaveEvents.PeriodicSave))
                result.Append($"\nSave Period: {savePeriod} sec");
            result.Append($"\nPlayer Tag: {playerTag}");
            return result.ToString();
        }


        public void Dispose () {
            Resources.UnloadAsset(this);
        }

    }

}