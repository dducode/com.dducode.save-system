using System.Collections;
using Cysharp.Threading.Tasks;
using SaveSystem.Handlers;
using SaveSystem.Tests.TestObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace SaveSystem.Tests {

    public class RemoteTests {

        private const string URL = "https://127.0.0.1:80/fake_remote_storage/test.bytes";


        [UnityTest]
        public IEnumerator SendDataToRemote () => UniTask.ToCoroutine(async () => {
            var firstObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            RemoteHandler<BinaryObject> handler = ObjectHandlersFactory.CreateRemoteHandler(URL, firstObject);
            await handler.SaveAsync();
        });

    }

}