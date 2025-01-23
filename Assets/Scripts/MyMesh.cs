using System.Collections.Generic;
using UnityEngine;
using Vertex = SelectionManager.Vertex;

public class MyMesh : MonoBehaviour
{

    public List<Vertex> vertices = new();
    
    // Start is called before the first frame update
    void Start()
    {
        //tmp initialize to a quad
        vertices.Add(new Vertex(new Vector3(-0.5f, -0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(0.5f, -0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(0.5f, 0.5f, 0)));
        vertices.Add(new Vertex(new Vector3(-0.5f, 0.5f, 0)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
