using UnityEngine;
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
}
