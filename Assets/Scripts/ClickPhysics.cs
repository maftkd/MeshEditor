using UnityEngine;
using SelectionMode = SelectionManager.SelectionMode;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;

public class ClickPhysics : MonoBehaviour
{
    public MyMesh mesh;
    private float vertexWidthPercentage = 0.01f;
    private Camera _cam;
    
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
    }

    public ISelectionPrimitive Raycast(Ray ray, SelectionMode selectionMode)
    {
        switch (selectionMode)
        {
            case SelectionMode.Vertex:
                return RaycastVertices(ray);
            default:
                return null;
        }
    }

    private Vertex RaycastVertices(Ray ray)
    {
        float minT = float.MaxValue;
        Vertex closestVertex = null;
        
        foreach (Vertex v in mesh.vertices)
        {
            //this plane raycast is dual purpose
            //1) it confirms that the sphere is in front of the camera
            //2) it gives us the z-distance of the vertex in view space
            Plane plane = new Plane(_cam.transform.forward, v.position);
            Ray camForwardRay = new Ray(_cam.transform.position, _cam.transform.forward);
            if (plane.Raycast(camForwardRay, out float distance))
            {
                //use z-dist to scale the radius such that all vertices have equal size on screen
                float width = Mathf.Tan(_cam.fieldOfView * Mathf.Deg2Rad) * distance;
                float radius = width * vertexWidthPercentage;
                
                Vector2 hit = SphIntersect(ray.origin, ray.direction, v.position, radius);
                if (hit.y > 0.0)
                {
                    if(hit.x < minT)
                    {
                        minT = hit.x;
                        closestVertex = v;
                    }
                }
            }
        }

        return closestVertex;
    }
    
    public static bool RaycastMouseToPlaneAtPoint(Vector3 point, Camera cam, out Vector3 hit)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(cam.transform.forward, point);
        if (plane.Raycast(ray, out float distance))
        {
            hit = ray.GetPoint(distance);
            return true;
        }

        hit = Vector3.zero;
        return false;
    }
    
    // sphere of size ra centered at point ce
    // from https://iquilezles.org/articles/intersectors/
    Vector2 SphIntersect(Vector3 ro,Vector3 rd, Vector3 ce, float ra )
    {
        Vector3 oc = ro - ce;
        float b = Vector3.Dot( oc, rd );
        float c = Vector3.Dot( oc, oc ) - ra*ra;
        float h = b*b - c;
        if( h<0.0 ) return new Vector2(-1.0f, -1.0f); // no intersection
        h = Mathf.Sqrt( h );
        return new Vector2( -b-h, -b+h );
    }
}
