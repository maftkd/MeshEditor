using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformGizmo : MonoBehaviour
{
    public Action<Vector3> OnTranslated;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Translate(Vector3 newPos)
    {
        OnTranslated?.Invoke(newPos);
    }
}
