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
        //tmp initialize to a cube
        vertices.Add(new Vertex(new Vector3(-0.5f, -0.5f, -0.5f))); //front bottom left
        vertices.Add(new Vertex(new Vector3(0.5f, -0.5f, -0.5f))); //front bottom right
        vertices.Add(new Vertex(new Vector3(0.5f, 0.5f, -0.5f))); //front top right
        vertices.Add(new Vertex(new Vector3(-0.5f, 0.5f, -0.5f))); //front top left
        
        vertices.Add(new Vertex(new Vector3(-0.5f, -0.5f, 0.5f))); //back bottom left
        vertices.Add(new Vertex(new Vector3(0.5f, -0.5f, 0.5f))); //back bottom right
        vertices.Add(new Vertex(new Vector3(0.5f, 0.5f, 0.5f))); //back top right
        vertices.Add(new Vertex(new Vector3(-0.5f, 0.5f, 0.5f))); //back top left
        
        CreateFace(vertices[0], vertices[1], vertices[2], vertices[3]); //front face
        CreateFace(vertices[1], vertices[5], vertices[6], vertices[2]); //right face
        CreateFace(vertices[5], vertices[4], vertices[7], vertices[6]); //back face
        CreateFace(vertices[4], vertices[0], vertices[3], vertices[7]); //left face
        CreateFace(vertices[3], vertices[2], vertices[6], vertices[7]); //top face
        CreateFace(vertices[4], vertices[5], vertices[1], vertices[0]); //bottom face
    }

    void CreateFace(Vertex a, Vertex b, Vertex c, Vertex d)
    {
        //edges
        edges.Add(new Edge(a, b));
        edges.Add(new Edge(b, c));
        edges.Add(new Edge(c, d));
        edges.Add(new Edge(d, a));
        
        //loops
        loops.Add(new Loop(a, edges[^4]));
        loops.Add(new Loop(b, edges[^3]));
        loops.Add(new Loop(c, edges[^2]));
        loops.Add(new Loop(d, edges[^1]));
        
        //polygon
        polygons.Add(new Polygon(loops.Count - 4, 4));
        PolygonHelper.CalculateNormal(polygons[^1], this);
        PolygonHelper.Triangulate(polygons[^1], this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
