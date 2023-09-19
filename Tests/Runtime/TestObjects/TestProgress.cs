using System;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    public class TestProgress : IProgress<float> {

        public void Report (float value) {
            Debug.Log($"Saving progress: {value}");
        }

    }

}