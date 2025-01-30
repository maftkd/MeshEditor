using UnityEngine;
using Edge = SelectionManager.Edge;

public class EdgeRenderer : MonoBehaviour
{
    public Shader shader;
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
            _mat = new Material(shader);
            _mat.color = Color.black;
        }

        _mat.SetPass(0);
        
        GL.PushMatrix();
        
        GL.Begin(GL.LINES);
        foreach (Edge e in mesh.edges)
        {
            if (e.selected)
            {
                GL.Color(Color.green);
            }
            else
            {
                GL.Color(Color.black);
                
            }
            GL.Vertex(e.a.position);
            GL.Vertex(e.b.position);
        }
        GL.End();
        
        GL.PopMatrix();
    }
}
