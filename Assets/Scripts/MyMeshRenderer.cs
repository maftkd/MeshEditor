using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MyMeshRenderer : MonoBehaviour
{
    public Shader myVertexShader;
    private Material _mat;
    public Mesh quadMesh;
    public MyMesh myMesh;
    
    // Start is called before the first frame update
    void Start()
    {
        _mat = new Material(myVertexShader);
        _mat.enableInstancing = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (myMesh.vertices.Count > 0)
        {
            int numInstances = myMesh.vertices.Count;
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            Matrix4x4[] matrices = new Matrix4x4[numInstances];
            for (int i = 0; i < numInstances; ++i)
            {
                matrices[i] = Matrix4x4.Translate(transform.position + myMesh.vertices[i].position) * Matrix4x4.Rotate(Quaternion.identity) *
                              Matrix4x4.Scale(Vector3.one);
            }

            Graphics.DrawMeshInstanced(quadMesh, 0, _mat, matrices, numInstances, materialPropertyBlock, ShadowCastingMode.Off, false);
        }
    }
}
