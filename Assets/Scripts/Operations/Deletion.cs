using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

public class Deletion : MonoBehaviour
{
    public SelectionManager selectionManager;
    public MyMesh myMesh;
    
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
        if(Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)))
        {
            List<ISelectionPrimitive> deletedPrimitives = new List<ISelectionPrimitive>(selectionManager.selection);
            for (int i = selectionManager.selection.Count - 1; i >= 0; i--)
            {
                myMesh.vertices.Remove((SelectionManager.Vertex)selectionManager.selection[i]);
            }
            selectionManager.ClearSelection();
            UndoRedoStack.Instance.Push(new DeleteAction(deletedPrimitives));
        }
    }
    
    void OnUndoRedo(IInputAction action, bool isUndo)
    {
        if (action is DeleteAction deleteAction)
        {
            if (isUndo)
            {
                foreach (var primitive in deleteAction.deletedPrimitives)
                {
                    myMesh.vertices.Add((SelectionManager.Vertex)primitive);
                }
                selectionManager.SetSelection(deleteAction.deletedPrimitives);
            }
            else
            {
                foreach (var primitive in deleteAction.deletedPrimitives)
                {
                    myMesh.vertices.Remove((SelectionManager.Vertex)primitive);
                }
                selectionManager.ClearSelection();
            }
        }
    }
}
