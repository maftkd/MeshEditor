using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

public class Duplication : MonoBehaviour
{
    public SelectionManager selectionManager;
    public MyMesh myMesh;
    public UnityEvent OnDuplicate;
    // Start is called before the first frame update
    void Start()
    {
        UndoRedoStack.Instance.UndoRedo += OnUndoRedo;
    }

    private void OnDestroy()
    {
        if (UndoRedoStack.Instance != null)
        {
            UndoRedoStack.Instance.UndoRedo -= OnUndoRedo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl) 
                                                                             || Input.GetKey(KeyCode.LeftShift)))
        {
            List<ISelectionPrimitive> duplicatedPrimitives = new List<ISelectionPrimitive>();
            List<ISelectionPrimitive> previousSelection = new List<ISelectionPrimitive>(selectionManager.selection);
            for (int i = selectionManager.selection.Count - 1; i >= 0; i--)
            {
                ISelectionPrimitive prim = selectionManager.selection[i].Copy();
                duplicatedPrimitives.Add(prim);
                myMesh.vertices.Add((SelectionManager.Vertex)prim);
            }
            selectionManager.SetSelection(duplicatedPrimitives);
            //UndoRedoStack.Instance.Push(new DuplicateAction(duplicatedPrimitives, previousSelection));
            OnDuplicate?.Invoke();
        }
    }
    
    void OnUndoRedo(IInputAction action, bool isUndo)
    {
        if (action is DuplicateAction duplicateAction)
        {
            if (isUndo)
            {
                foreach (var primitive in duplicateAction.duplicatedPrimitives)
                {
                    myMesh.vertices.Remove((SelectionManager.Vertex)primitive);
                }
                selectionManager.SetSelection(duplicateAction.previousSelection);
            }
            else
            {
                foreach (var primitive in duplicateAction.duplicatedPrimitives)
                {
                    myMesh.vertices.Add((SelectionManager.Vertex)primitive);
                }
                selectionManager.SetSelection(duplicateAction.duplicatedPrimitives);
            }
        }
    }
}
