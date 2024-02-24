using System;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    [RequireComponent(typeof(Rigidbody))]
    public class TestRigidbody : MonoBehaviour {

        [NonSerialized]
        public Vector3 position;


        private void Update () {
            position = transform.position;
        }

    }

}