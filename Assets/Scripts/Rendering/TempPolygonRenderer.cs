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
    
    private void OnPostRender()
    {
        if (_mat == null)
        {
            Shader tmpShader = Shader.Find("Unlit/Color");
            _mat = new Material(tmpShader);
            _mat.color = Color.red;
        }

        _mat.SetPass(0);
        
        GL.PushMatrix();
        
        GL.Begin(GL.QUADS);
        foreach (Polygon poly in mesh.polygons)
        {
            for(int i = poly.loopStartIndex; i < poly.loopStartIndex + poly.numLoops; i++)
            {
                Loop loop = mesh.loops[i];
                GL.Vertex(loop.start.position);
            }
            /*
            switch (selectionManager.selectionMode)
            {
                case SelectionMode.Vertex:
                    GL.Color(e.a.selected ? Color.green : Color.black);
                    GL.Vertex(e.a.position);
                    GL.Color(e.b.selected ? Color.green : Color.black);
                    GL.Vertex(e.b.position);
                    break;
                default:
                    GL.Color(e.selected ? Color.green : Color.black);
                    GL.Vertex(e.a.position);
                    GL.Vertex(e.b.position);
                    break;
            }
            */
        }
        GL.End();
        
        GL.PopMatrix();
    }
}
