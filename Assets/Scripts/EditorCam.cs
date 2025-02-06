using System.Collections;
using UnityEngine;

/// <summary>
/// Based on Unity's editor camera controls
/// Caches position and rotation so when we go back into editor cam, we see the same view
/// </summary>
public class EditorCam : MonoBehaviour
{
    public float cameraOrbitSpeed;
    public float cameraMoveSpeed;

    private Vector3 _focusPoint;
    
    public SelectionManager selectionManager;
    
    // Update is called once per frame
    void Update()
    {
        selectionManager.selectionDisabledViaCamera = false;

        //frame selected
        if (Input.GetKeyDown(KeyCode.Period))
        {
            if (selectionManager.GetSelectionCenterAndBounds(out Vector3 center, out Bounds bounds))
            {
                float maxBounds = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                StopAllCoroutines();
                StartCoroutine(MoveToNewFocusPoint(center, maxBounds));
            }

            return;
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //mouse movement to rotate
            //float mouseX = Input.GetAxis("Mouse X");
            //float mouseY = Input.GetAxis("Mouse Y");
            float mouseX = Input.mouseScrollDelta.x;
            float mouseY = Input.mouseScrollDelta.y;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            mouseX *= 0.1f;
            mouseY *= 0.1f;
            #endif

            if (mouseX == 0 && mouseY == 0)
            {
                return;
            }
            
            selectionManager.selectionDisabledViaCamera = true;

            //movement
            Vector3 movement = Vector3.zero;
            movement += Vector3.up * mouseY;
            movement -= transform.right * mouseX;

            if (movement != Vector3.zero)
            {
                Vector3 delta = movement * cameraMoveSpeed;
                transform.position += delta;
                _focusPoint += delta;
            }
        }
        //zoom
        else if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 movement = (_focusPoint - transform.position);
            Vector3 newPos = transform.position + movement.normalized * Input.mouseScrollDelta.y;
            if(Vector3.Distance(newPos, _focusPoint) > 0.1f)
            {
                transform.position = newPos;
                transform.LookAt(_focusPoint);
            }
        }
        //orbit
        else if (Input.mouseScrollDelta != Vector2.zero)
        {
            selectionManager.selectionDisabledViaCamera = true;
            //orbit around focus point
            //mouse movement to rotate
            float mouseX = Input.mouseScrollDelta.x;
            float mouseY = Input.mouseScrollDelta.y;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            mouseX *= 0.1f;
            mouseY *= 0.1f;
            #endif

            Vector3 diff = (transform.position - _focusPoint);
            float radius = diff.magnitude;
            float xzLength = Mathf.Sqrt(diff.x * diff.x + diff.z * diff.z);
            float phi = Mathf.Atan2(diff.y, xzLength);
            float theta = Mathf.Atan2(diff.z, diff.x);
            theta -= mouseX * cameraOrbitSpeed;
            phi += mouseY * cameraOrbitSpeed;
            phi = Mathf.Clamp(phi, -Mathf.PI / 2 + 0.01f, Mathf.PI / 2 - 0.01f);
            
            transform.position = _focusPoint + new Vector3(radius * Mathf.Cos(phi) * Mathf.Cos(theta), 
                radius * Mathf.Sin(phi), radius * Mathf.Cos(phi) * Mathf.Sin(theta));
            transform.LookAt(_focusPoint);
        }
    }

    IEnumerator MoveToNewFocusPoint(Vector3 newPoint, float maxBounds)
    {
        Vector3 targetPos = newPoint - transform.forward * (maxBounds * 2f);
        Vector3 startPos = transform.position;
        _focusPoint = newPoint;
        float duration = 0.3f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, timer / duration);
            yield return null;
        }
        transform.position = targetPos;
    }
}
