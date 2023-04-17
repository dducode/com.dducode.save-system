using UnityEngine;

namespace SaveSystem.Tests.Runtime {

    public class Showing : IProgress {

        public void Show (float progress) {
            Debug.Log(progress);
        }

    }

}