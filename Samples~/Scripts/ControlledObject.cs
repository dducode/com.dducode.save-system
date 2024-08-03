using Cysharp.Threading.Tasks;
using SaveSystemPackage.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;


public class ControlledObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [SerializeField]
    private ObjectsController controller;

    [SerializeField, NonEditable]
    private Vector3 startPosition;

    private Rigidbody m_rb;
    private Collider m_collider;
    private bool m_performed;


    private void Awake () {
        m_rb = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        startPosition = transform.position;
    }


    public void OnValidate () {
        startPosition = transform.position;
    }


    public void OnBeginDrag (PointerEventData eventData) {
        controller.StartMove(eventData, m_rb);
    }


    public void OnDrag (PointerEventData eventData) {
        controller.Move(eventData);
    }


    public void OnEndDrag (PointerEventData eventData) {
        controller.EndMove();
    }


    public async void OnReset () {
        if (m_performed)
            return;

        m_performed = true;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        var t = 0f;
        m_collider.enabled = false;
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;
        m_rb.isKinematic = true;

        while (t < 1) {
            if (destroyCancellationToken.IsCancellationRequested)
                return;
            transform.position = Vector3.Lerp(position, startPosition, t);
            transform.rotation = Quaternion.Lerp(rotation, Quaternion.identity, t);
            t += Time.fixedDeltaTime * 2;
            await UniTask.WaitForFixedUpdate(destroyCancellationToken).SuppressCancellationThrow();
        }

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
        m_collider.enabled = true;
        m_rb.isKinematic = false;
        m_performed = false;
    }

}