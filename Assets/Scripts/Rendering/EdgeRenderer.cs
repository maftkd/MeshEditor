using UnityEngine;
using Edge = SelectionManager.Edge;
using SelectionMode = SelectionManager.SelectionMode;

public class EdgeRenderer : MonoBehaviour
{
    public Shader shader;
    private Material _mat;
    public MyMesh mesh;

    public SelectionManager selectionManager;
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
            /*
            if (e.selected)
            {
                GL.Color(Color.green);
            }
            else
            {
                GL.Color(Color.black);
            }
            */
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
        }
        GL.End();
        
        GL.PopMatrix();
    }
}
