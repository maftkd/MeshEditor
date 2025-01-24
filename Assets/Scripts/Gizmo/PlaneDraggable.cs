using UnityEngine;
using UnityEngine.Events;

public class PlaneDraggable : MonoBehaviour
{
    private bool _dragging = false;
    public Transform moveTarget;

    private Camera mainCam;

    public UnityEvent<Vector3> OnDrag;
    public UnityEvent OnDragComplete;

    public GameObject visualGameObject;

    private Vector3 _offset;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        if (visualGameObject != null)
        {
            visualGameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_dragging)
        {
            
            moveTarget.transform.position = GetPointOnPlaneNearMouse() + _offset;
            
            OnDrag?.Invoke(moveTarget.position);
            
            if (Input.GetMouseButtonUp(0))
            {
                _dragging = false;
                if (visualGameObject != null)
                {
                    visualGameObject.SetActive(false);
                }
                OnDragComplete?.Invoke();
            }
        }
    }

    private Vector3 GetPointOnPlaneNearMouse()
    {
        Plane plane = new Plane(transform.up, moveTarget.position);
        
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void OnMouseDown()
    {
        _dragging = true;
        if (visualGameObject != null)
        {
            visualGameObject.SetActive(true);
        }

        Vector3 initialPos = GetPointOnPlaneNearMouse();
        _offset = moveTarget.position - initialPos;
    }


    private void OnDrawGizmos()
    {
    }
}
