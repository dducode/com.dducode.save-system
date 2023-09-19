using UnityEngine;

namespace SaveSystem.Core {

    public class SaveSystemSettings : ScriptableObject {

        public bool autoSaveEnabled;

        [Min(0)]
        public float savePeriod;

        public bool asyncSaveEnabled;
        public bool debugEnabled;
        public bool destroyCheckPoints;
        public string playerTag;

    }

}