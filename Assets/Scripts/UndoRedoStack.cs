using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoStack : MonoBehaviour
{
    private Stack<IInputAction> _undoStack = new();
    private Stack<IInputAction> _redoStack = new();

    public Action<IInputAction, bool> UndoRedo;
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
            Redo();
        }
        else if((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
    }

    void Undo()
    {
        if (_undoStack.Count > 0)
        {
            IInputAction action = _undoStack.Pop();
            _redoStack.Push(action);
            UndoRedo(action, true);
        }
    }

    void Redo()
    {
        if(_redoStack.Count > 0)
        {
            IInputAction action = _redoStack.Pop();
            _undoStack.Push(action);
            UndoRedo(action, false);
        }
    }
}
