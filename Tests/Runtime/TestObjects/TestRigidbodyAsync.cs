using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;
using UnityEngine;

namespace SaveSystem.Tests.TestObjects {

    [RequireComponent(typeof(Rigidbody))]
    public class TestRigidbodyAsync : MonoBehaviour, IPersistentObjectAsync {

        private Vector3 m_position;


        private void Update () {
            m_position = transform.position;
        }


        public async UniTask Save (UnityWriter writer) {
            await writer.WriteAsync(new[] {m_position});
        }


        public async UniTask Load (UnityReader reader) {
            transform.position = (await reader.ReadVector3ArrayAsync())[0];
        }

    }

}