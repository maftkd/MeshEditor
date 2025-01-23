using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMesh : MonoBehaviour
{
    public class Vertex
    {
        public Vector3 position;

        public Vertex(Vector3 pos)
        {
            position = pos;
        }
    }

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
