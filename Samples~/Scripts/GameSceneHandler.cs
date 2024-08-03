using Cysharp.Threading.Tasks;
using SaveSystemPackage;
using UnityEngine;



public class GameSceneHandler : SceneHandler<SceneData> {

    public async void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            await sceneContext.Save();
    }


    public override void StartScene (SceneData data) {
        Debug.Log(data.data);
        sceneContext.Load().Forget();
        DynamicGI.UpdateEnvironment();
    }

}