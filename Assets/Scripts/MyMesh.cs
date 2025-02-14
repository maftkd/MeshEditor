using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Loop = SelectionManager.Loop;
using Polygon = SelectionManager.Polygon;

public class MyMesh : MonoBehaviour
{
    public List<Vertex> vertices = new();
    public List<Edge> edges = new();
    public List<Loop> loops = new();
    public List<Polygon> polygons = new();
    
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
        
        //loops
        loops.Add(new Loop(vertices[0], edges[0]));
        loops.Add(new Loop(vertices[1], edges[1]));
        loops.Add(new Loop(vertices[2], edges[2]));
        loops.Add(new Loop(vertices[3], edges[3]));
        
        //polygons
        polygons.Add(new Polygon(0, 4));
        PolygonHelper.CalculateNormal(polygons[0], this);
        PolygonHelper.Triangulate(polygons[0], this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
