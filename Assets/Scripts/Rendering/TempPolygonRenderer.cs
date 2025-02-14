using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Loop = SelectionManager.Loop;
using Polygon = SelectionManager.Polygon;

public class TempPolygonRenderer : MonoBehaviour
{
    private Material _mat;
    public MyMesh mesh;
    public SelectionManager selectionManager;
    private Camera _cam;
    public Vector3 lightDirViewSpace;

    private void Start()
    {
        _cam = GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        if (_mat == null)
        {
            Shader tmpShader = Shader.Find("Unlit/Edge");
            _mat = new Material(tmpShader);
        }

        _mat.SetPass(0);
        
        GL.PushMatrix();
        
        GL.Begin(GL.TRIANGLES);
        foreach (Polygon poly in mesh.polygons)
        {
            Vector3 viewNorm = _cam.worldToCameraMatrix * new Vector4(poly.normal.x, poly.normal.y, poly.normal.z, 0.0f);
            float dt = -Vector3.Dot(viewNorm, lightDirViewSpace.normalized) * 0.5f + 0.5f;
            float lighting = Mathf.Lerp(0.3f, 0.6f, dt);
            Color baseCol = poly.selected ? new Color(0.75f, 1.5f, 0.75f) : Color.white;
            GL.Color(baseCol * lighting);
            foreach (Vertex v in poly.tris)
            {
                GL.Vertex(v.position);
            }
            /*
            for(int i = poly.loopStartIndex; i < poly.loopStartIndex + poly.numLoops; i++)
            {
                Loop loop = mesh.loops[i];
                GL.Vertex(loop.start.position);
            }
            */
        }
        GL.End();
        
        GL.PopMatrix();
    }
}
