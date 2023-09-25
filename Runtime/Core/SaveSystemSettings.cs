using UnityEngine;

namespace SaveSystem.Core {

    public class SaveSystemSettings : ScriptableObject {

        [Tooltip("Enables logs" +
                 "\nIt configures only simple logs, other logs (warnings and errors) will be written to console anyway.")]
        public bool debugEnabled;

        [Tooltip("It's used to enable/disable autosave loop")]
        public bool autoSaveEnabled;

        [Min(0)]
        [Tooltip("It's used into autosave loop to determine saving frequency")]
        public float savePeriod;

        [Tooltip("You can choose 3 saving modes - simple mode, async saving and multithreading saving (parallel)")]
        public SaveMode saveMode;

        [Tooltip("Defines method to save simple objects (in the main thread or in the thread pool)" +
                 "\nIt's only used when async save mode is selected. Otherwise it'll be ignored")]
        public AsyncMode asyncMode;

        [Tooltip("Determines whether checkpoints will be destroyed after saving")]
        public bool destroyCheckPoints;

        [Tooltip("Player tag is used to filtering messages from triggered checkpoints")]
        public string playerTag;

        [Tooltip("Configure it to automatically register handlers in the Save System Core after creation")]
        public bool registerImmediately;

    }

}