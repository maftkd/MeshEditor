using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;
using Polygon = SelectionManager.Polygon;
using Loop = SelectionManager.Loop;

public class PolygonHelper
{
    public static void CalculateNormal(Polygon poly, MyMesh mesh)
    {
        Vector3 center = Vector3.zero;
        for(int i = poly.loopStartIndex; i < poly.loopStartIndex + poly.numLoops; i++)
        {
            Loop loop = mesh.loops[i];
            center += loop.start.position;
        }
        
        center /= poly.numLoops;
        
        Vector3 r0 = (mesh.loops[poly.loopStartIndex].start.position - center).normalized;
        Vector3 r1 = (mesh.loops[poly.loopStartIndex + 1].start.position - center).normalized;
        
        poly.normal = Vector3.Cross(r0, r1).normalized;
    }
    
    //using a version of earclipping as described in http://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
    //and visualized https://cedric-h.github.io/linear-webgl/earclip.html
    public static void Triangulate(Polygon poly, MyMesh mesh)
    {
        List<Vertex> triangles = new List<Vertex>();
        
        //a rotation for the vertices so that they lie in the x-y plane
        Quaternion rot = Quaternion.FromToRotation(poly.normal, Vector3.forward);
        Quaternion inverseRot = Quaternion.Inverse(rot);
        
        //first collect the vertices of the polygon
        List<Vertex> vertices = new List<Vertex>();
        for(int i = poly.loopStartIndex; i < poly.loopStartIndex + poly.numLoops; i++)
        {
            Vertex v = mesh.loops[i].start;
            //and rotate all the points so that they lie in the x-y plane
            v.position = rot * v.position;
            vertices.Add(v);
        }
        
        //iterate through until we have only 3 verts remaining
        while (vertices.Count > 3)
        {
            bool earFound = false;
            //check for an ear and break if we find one
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 vert = vertices[i].position;
                int prevIndex = (i - 1 + vertices.Count) % vertices.Count;
                Vector2 prev = vertices[prevIndex].position;
                int nextIndex = (i + 1) % vertices.Count;
                Vector2 next = vertices[nextIndex].position;
                
                //check determinant to make sure vertex is convex
                if (!IsConvex(prev, vert, next))
                {
                    continue;
                }

                //skip if any other vertex is inside the triangle
                bool intersectionTest = true;
                for(int j = 0; j < vertices.Count; j++)
                {
                    if (j == i || j == prevIndex || j == nextIndex)
                    {
                        continue;
                    }
                    
                    Vector2 test = vertices[j].position;
                    if (PointInTriangle(prev, vert, next, test))
                    {
                        intersectionTest = false;
                        break;
                    }
                }
                if (!intersectionTest)
                {
                    continue;
                }

                earFound = true;
                triangles.Add(vertices[prevIndex]);
                triangles.Add(vertices[i]);
                triangles.Add(vertices[nextIndex]);
                vertices.RemoveAt(i);
                break;
            }

            if (!earFound)
            {
                Debug.LogError("Checked all verts, and couldn't find an ear");
                break;
            }
        }
        
        //finally add the last three verts
        triangles.Add(vertices[0]);
        triangles.Add(vertices[1]);
        triangles.Add(vertices[2]);
        
        //and un-rotate all vertices
        for(int i = poly.loopStartIndex; i < poly.loopStartIndex + poly.numLoops; i++)
        {
            Vertex v = mesh.loops[i].start;
            v.position = inverseRot * v.position;
        }
        
        Debug.Log($"Found {triangles.Count} triangles");
        poly.tris = triangles;
    }

    public static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        float d1x = a.x - b.x;
        float d1y = a.y - b.y;
        float d2x = b.x - c.x;
        float d2y = b.y - c.y;
        if ((d1x*d2y - d1y*d2x) < 0) {
            return false;
        }

        return true;
    }
    
    public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 s)
    {
        float as_x = s.x - a.x;
        float as_y = s.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) 
            return false;
        if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y)*(s.x - b.x) > 0 != s_ab) 
            return false;
        return true;
    }
}
