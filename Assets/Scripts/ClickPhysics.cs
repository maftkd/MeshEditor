using System.Collections.Generic;
using UnityEngine;
using SelectionMode = SelectionManager.SelectionMode;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Polygon = SelectionManager.Polygon;

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
            case SelectionMode.Edge:
                return RaycastEdges(ray);
            case SelectionMode.Face:
                return RaycastFaces(ray);
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
    private Edge RaycastEdges(Ray ray)
    {
        float minT = float.MaxValue;
        Edge closestEdge = null;
        
        foreach (Edge e in mesh.edges)
        {
            Vector3 center = (e.a.position + e.b.position) / 2;
            //this plane raycast is dual purpose
            //1) it confirms that the sphere is in front of the camera
            //2) it gives us the z-distance of the vertex in view space
            Plane plane = new Plane(_cam.transform.forward, center);
            Ray camForwardRay = new Ray(_cam.transform.position, _cam.transform.forward);
            if (plane.Raycast(camForwardRay, out float distance))
            {
                //use z-dist to scale the radius such that all vertices have equal size on screen
                float width = Mathf.Tan(_cam.fieldOfView * Mathf.Deg2Rad) * distance;
                float radius = width * vertexWidthPercentage;
                
                float hit = CapIntersect(ray.origin, ray.direction, e.a.position, e.b.position, radius);
                if (hit > 0.0)
                {
                    if(hit < minT)
                    {
                        minT = hit;
                        closestEdge = e;
                    }
                }
            }
        }

        return closestEdge;
    }
    
    private Polygon RaycastFaces(Ray ray)
    {
        float minT = float.MaxValue;
        Polygon closestPoly = null;
        
        foreach (Polygon p in mesh.polygons)
        {
            for (int i = 0; i < p.tris.Count; i += 3)
            {
                Vector3 v1 = p.tris[i].position;
                Vector3 v2 = p.tris[i + 1].position;
                Vector3 v3 = p.tris[i + 2].position;
                
                Vector3 hit = TriIntersect(ray.origin, ray.direction, v1, v2, v3);
                if (hit.x > 0.0)
                {
                    if(hit.x < minT)
                    {
                        minT = hit.x;
                        closestPoly = p;
                    }
                }
            }
        }

        return closestPoly;
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
    

    //box xy is min, zw is max, all in normalized 0 to 1 space
    public List<ISelectionPrimitive> FrustumOverlap(Vector4 boxCoords, SelectionMode selectionMode)
    {
        switch (selectionMode)
        {
            case SelectionMode.Vertex:
                return FrustumOverlapVertices(boxCoords);
                break;
            case SelectionMode.Edge:
                return FrustumOverlapEdges(boxCoords);
                break;
        }
        return null;
    }

    List<ISelectionPrimitive> FrustumOverlapVertices(Vector4 boxCoords)
    {
        List<ISelectionPrimitive> selection = new();
        foreach (Vertex v in mesh.vertices)
        {
            float xMin = Mathf.Lerp(-1f, 1f, boxCoords.x);
            float xMax = Mathf.Lerp(-1f, 1f, boxCoords.z);
            float yMin = Mathf.Lerp(-1f, 1f, boxCoords.y);
            float yMax = Mathf.Lerp(-1f, 1f, boxCoords.w);
            if (PointIsInsideFrustumRegion(xMin, xMax, yMin, yMax, v.position, out Vector4 clipPos))
            {
                selection.Add(v);
            }
        }

        return selection;
    }
    
    bool PointIsInsideFrustumRegion(float xMin, float xMax, float yMin, float yMax, Vector4 worldPos, out Vector4 clipPos)
    {
        clipPos = Vector4.zero;
        Vector4 viewPos = _cam.worldToCameraMatrix * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1.0f);
        if(-viewPos.z < _cam.nearClipPlane || -viewPos.z > _cam.farClipPlane)
        {
            return false;
        }
        clipPos = _cam.projectionMatrix * viewPos;
        clipPos /= clipPos.w;
        return clipPos.x >= xMin && clipPos.x <= xMax && clipPos.y >= yMin && clipPos.y <= yMax;
    }
    
    List<ISelectionPrimitive> FrustumOverlapEdges(Vector4 boxCoords)
    {
        List<ISelectionPrimitive> selection = new();
        foreach (Edge e in mesh.edges)
        {
            float xMin = Mathf.Lerp(-1f, 1f, boxCoords.x);
            float xMax = Mathf.Lerp(-1f, 1f, boxCoords.z);
            float yMin = Mathf.Lerp(-1f, 1f, boxCoords.y);
            float yMax = Mathf.Lerp(-1f, 1f, boxCoords.w);
            if (PointIsInsideFrustumRegion(xMin, xMax, yMin, yMax, e.a.position, out Vector4 clipA))
            {
                selection.Add(e);
                continue;
            }

            if (PointIsInsideFrustumRegion(xMin, xMax, yMin, yMax, e.b.position, out Vector4 clipB))
            {
                selection.Add(e);
                continue;
            }
            
            if(clipA == Vector4.zero || clipB == Vector4.zero)
            {
                continue;
            }
            
            //check if the edge intersects the box
            if (SegmentIntersectRectangle(xMin, xMax, yMin, yMax, clipA.x, clipA.y, clipB.x, clipB.y))
            {
                selection.Add(e);
            }
        }

        return selection;
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
    float CapIntersect( in Vector3 ro, in Vector3 rd, in Vector3 pa, in Vector3 pb, in float ra )
    {
        Vector3  ba = pb - pa;
        Vector3  oa = ro - pa;
        float baba = Vector3.Dot(ba,ba);
        float bard = Vector3.Dot(ba,rd);
        float baoa = Vector3.Dot(ba,oa);
        float rdoa = Vector3.Dot(rd,oa);
        float oaoa = Vector3.Dot(oa,oa);
        float a = baba      - bard*bard;
        float b = baba*rdoa - baoa*bard;
        float c = baba*oaoa - baoa*baoa - ra*ra*baba;
        float h = b*b - a*c;
        if( h >= 0.0 )
        {
            float t = (-b-Mathf.Sqrt(h))/a;
            float y = baoa + t*bard;
            // body
            if( y>0.0 && y<baba ) return t;
            // caps
            Vector3 oc = (y <= 0.0) ? oa : ro - pb;
            b = Vector3.Dot(rd,oc);
            c = Vector3.Dot(oc,oc) - ra*ra;
            h = b*b - c;
            if( h>0.0 ) return -b - Mathf.Sqrt(h);
        }
        return -1.0f;
    }
    
    Vector3 TriIntersect( in Vector3 ro, in Vector3 rd, in Vector3 v0, in Vector3 v1, in Vector3 v2 )
    {
        Vector3 v1v0 = v1 - v0;
        Vector3 v2v0 = v2 - v0;
        Vector3 rov0 = ro - v0;
        Vector3  n = Vector3.Cross( v1v0, v2v0 );
        Vector3  q = Vector3.Cross( rov0, rd );
        float d = 1.0f / Vector3.Dot( rd, n );
        float u = d*Vector3.Dot( -q, v2v0 );
        float v = d*Vector3.Dot(  q, v1v0 );
        float t = d*Vector3.Dot( -n, rov0 );
        if( u<0.0 || v<0.0 || (u+v)>1.0 ) t = -1.0f;
        return new Vector3( t, u, v );
    }
    
    // https://stackoverflow.com/questions/99353/how-to-test-if-a-line-segment-intersects-an-axis-aligned-rectange-in-2d
    // Answer by Metamal
    bool SegmentIntersectRectangle(float xMin, float xMax, float yMin, float yMax, float a_p1x, float a_p1y, float a_p2x, float a_p2y)
    {
            
        // Find min and max X for the segment
        float minX = a_p1x;
        float maxX = a_p2x;

        if(a_p1x > a_p2x)
        {
            minX = a_p2x;
            maxX = a_p1x;
        }

        // Find the intersection of the segment's and rectangle's x-projections
        if(maxX > xMax)
        {
            maxX = xMax;
        }

        if(minX < xMin)
        {
            minX = xMin;
        }
        
        // If their projections do not intersect return false
        if(minX > maxX) 
        {
            return false;
        }

        // Find corresponding min and max Y for min and max X we found before
        float minY = a_p1y;
        float maxY = a_p2y;

        float dx = a_p2x - a_p1x;

        if(Mathf.Abs(dx) > 0.0000001)
        {
            float a = (a_p2y - a_p1y) / dx;
            float b = a_p1y - a * a_p1x;
            minY = a * minX + b;
            maxY = a * maxX + b;
        }

        if(minY > maxY)
        {
            (maxY, minY) = (minY, maxY);
        }

        // Find the intersection of the segment's and rectangle's y-projections
        if(maxY > yMax)
        {
            maxY = yMax;
        }

        if(minY < yMin)
        {
            minY = yMin;
        }
        
        // If Y-projections do not intersect return false
        if(minY > maxY) 
        {
            return false;
        }

        return true;
    }
}
