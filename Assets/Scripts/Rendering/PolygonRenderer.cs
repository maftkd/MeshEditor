using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Loop = SelectionManager.Loop;
using Polygon = SelectionManager.Polygon;

public class PolygonRenderer : MonoBehaviour
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
        
        Color selectedColor = selectionManager.selectionMode == SelectionManager.SelectionMode.Face ? 
            new Color(0.75f, 1.5f, 0.75f) : new Color(1f, 1.15f, 1f);
        
        GL.PushMatrix();
        
        GL.Begin(GL.TRIANGLES);
        foreach (Polygon poly in mesh.polygons)
        {
            Vector3 viewNorm = _cam.worldToCameraMatrix * new Vector4(poly.normal.x, poly.normal.y, poly.normal.z, 0.0f);
            float dt = -Vector3.Dot(viewNorm, lightDirViewSpace.normalized) * 0.5f + 0.5f;
            float lighting = Mathf.Lerp(0.3f, 0.6f, dt);
            Color baseCol = poly.selected ? selectedColor : Color.white;
            GL.Color(baseCol * lighting);
            foreach (Vertex v in poly.tris)
            {
                GL.Vertex(v.position);
            }
        }
        GL.End();
        
        GL.PopMatrix();
    }
}
