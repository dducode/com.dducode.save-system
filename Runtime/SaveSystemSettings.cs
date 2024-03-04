using SaveSystem.Internal.Templates;
using UnityEngine;

namespace SaveSystem {

    public class SaveSystemSettings : ScriptableObject {

        public SaveEvents enabledSaveEvents = SaveEvents.AutoSave | SaveEvents.OnExit;
        public LogLevel enabledLogs = LogLevel.Warning | LogLevel.Error;

        [Min(0)]
        [Tooltip(Tooltips.SavePeriod)]
        public float savePeriod = 15;

        [Tooltip(Tooltips.IsParallel)]
        public bool isParallel;

        [Tooltip(Tooltips.DataPath)]
        public string dataPath = "default_data_file.data";

        public string playerTag = "Player";

    }

}