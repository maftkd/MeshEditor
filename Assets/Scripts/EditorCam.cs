using UnityEngine;

/// <summary>
/// Based on Unity's editor camera controls
/// Caches position and rotation so when we go back into editor cam, we see the same view
/// </summary>
public class EditorCam : MonoBehaviour
{
    public float cameraLookSpeed;
    public float cameraMoveSpeed;
    public float cameraBoostMult;
    public float cameraSpeedChangeSpeed;
    public float minSpeedMult;
    public float maxSpeedMult;

    private float baseSpeedMult = 2f;
    private Vector3 _prevPos;
    private Quaternion _prevRot;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            //mouse movement to rotate
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            mouseX *= 0.1f;
            mouseY *= 0.1f;
            #endif
            
            if(mouseX !=0 || mouseY != 0)
            {
                transform.forward = Quaternion.Euler(Vector3.up * (mouseX * cameraLookSpeed)) * transform.forward;
                transform.rotation *= Quaternion.AngleAxis(-mouseY * cameraLookSpeed, Vector3.right);
            }

            //movement
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                movement += transform.forward;
            }
            if(Input.GetKey(KeyCode.A))
            {
                movement -= transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                movement += transform.right;
            }
            if (Input.GetKey(KeyCode.S))
            {
                movement -= transform.forward;
            }
            if (Input.GetKey(KeyCode.E))
            {
                movement += Vector3.up;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                movement -= Vector3.up;
            }

            //scroll wheel to change base speed
            baseSpeedMult += Input.mouseScrollDelta.y * cameraSpeedChangeSpeed;
            baseSpeedMult = Mathf.Clamp(baseSpeedMult, minSpeedMult, maxSpeedMult);

            if (movement != Vector3.zero)
            {
                movement.Normalize();
                float speed = cameraMoveSpeed * baseSpeedMult * (Input.GetKey(KeyCode.LeftShift) ? cameraBoostMult : 1);
                transform.position += movement * (speed * Time.deltaTime);
            }
        }
    }
}
