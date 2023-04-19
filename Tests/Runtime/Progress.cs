using System;
using UnityEngine;

namespace SaveSystem.Tests.Runtime {

    public class Progress : IProgress<float> {

        public void Report (float progress) {
            Debug.Log(progress);
        }

    }

}