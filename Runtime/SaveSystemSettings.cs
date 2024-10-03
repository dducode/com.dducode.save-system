#if ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
#define ENABLE_BOTH_SYSTEMS
#endif

using System;
using System.Text;
using SaveSystemPackage.Compressing;
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
        public SaveEvents enabledSaveEvents;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 5;

        public SerializerType serializerType;
        public BaseSerializerType baseSerializerType;

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

        public CompressionSettings compressionSettings;
        public EncryptionSettings encryptionSettings;


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