using UnityEngine;
using UnityEngine.EventSystems;


public class ObjectsController : MonoBehaviour {

    [SerializeField]
    private Joint joint;


    public void StartMove (PointerEventData eventData, Rigidbody connectedBody) {
        joint.transform.position = eventData.pointerCurrentRaycast.worldPosition;
        joint.connectedBody = connectedBody;
    }


    public void Move (PointerEventData eventData) {
        Camera eventCamera = eventData.pressEventCamera;
        Vector3 position = eventCamera.WorldToScreenPoint(joint.transform.position);
        position += (Vector3)eventData.delta;
        joint.transform.position = eventCamera.ScreenToWorldPoint(position);
    }


    public void EndMove () {
        joint.transform.position = Vector3.zero;
        joint.connectedBody = null;
    }

}