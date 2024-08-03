using UnityEngine;


public abstract class Window : MonoBehaviour {

    [SerializeField]
    private Canvas canvas;


    public void Open () {
        canvas.enabled = true;
    }


    public virtual void Close () {
        canvas.enabled = false;
    }


    public virtual void Refresh () { }

}