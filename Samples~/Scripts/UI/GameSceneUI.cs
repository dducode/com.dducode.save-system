using UnityEngine;


public class GameSceneUI : MonoBehaviour {

    [SerializeField]
    private ControlledObject[] objects;


    public void OnReset () {
        foreach (ControlledObject obj in objects)
            obj.OnReset();
    }

}