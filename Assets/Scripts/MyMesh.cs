using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;

public class MyMesh : MonoBehaviour
{
    public List<Vertex> vertices = new();
    public List<Edge> edges = new();
    
    // Start is called before the first frame update
    void Start()
    {
        //tmp initialize to a quad
        vertices.Add(new Vertex(new Vector3(-0.5f, -0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(0.5f, -0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(0.5f, 0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(-0.5f, 0.5f, 0)));
        
        //edges
        edges.Add(new Edge(vertices[0], vertices[1]));
        edges.Add(new Edge(vertices[1], vertices[2]));
        edges.Add(new Edge(vertices[2], vertices[3]));
        edges.Add(new Edge(vertices[3], vertices[0]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
