using SaveSystem.UnityHandlers;
using UnityEngine;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;
#else
using TaskAlias = System.Threading.Tasks.Task;
#endif

namespace SaveSystem.Tests.TestObjects {

    [RequireComponent(typeof(Rigidbody))]
    public class TestRigidbodyAsync : MonoBehaviour, IPersistentObjectAsync {

        private Vector3 m_position;


        private void Update () {
            m_position = transform.position;
        }


        public async TaskAlias Save (UnityWriter writer) {
            await writer.WriteAsync(new[] {m_position});
        }


        public async TaskAlias Load (UnityReader reader) {
            transform.position = (await reader.ReadVector3ArrayAsync())[0];
        }

    }

}