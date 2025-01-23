using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoStack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKey(KeyCode.LeftShift)
                && Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Redo");
        }
        else if((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Undo");
        }
    }
}
