using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Edge = SelectionManager.Edge;

public class EdgeRenderer : MonoBehaviour
{
    private Material _mat;
    public MyMesh mesh;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnPostRender()
    {
        if (_mat == null)
        {
            Shader shader = Shader.Find("Unlit/Color");
            _mat = new Material(shader);
            _mat.color = Color.black;
        }

        _mat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        foreach (Edge e in mesh.edges)
        {
            GL.Vertex(e.a.position);
            GL.Vertex(e.b.position);
        }
        GL.End();
        GL.PopMatrix();
    }
}
