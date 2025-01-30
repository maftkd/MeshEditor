using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;

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
            for (int i = deletedPrimitives.Count - 1; i >= 0; i--)
            {
                ISelectionPrimitive prim = deletedPrimitives[i];
                switch (prim)
                {
                    case Vertex v:
                        myMesh.vertices.Remove(v);
                        
                        //removing a vertex that belongs to an edge results in deletion of that edge too
                        for (int j = myMesh.edges.Count - 1; j >= 0; j--)
                        {
                            Edge edge = myMesh.edges[j];
                            if (edge.a == v || edge.b == v)
                            {
                                myMesh.edges.Remove(edge);
                                deletedPrimitives.Add(edge);
                            }
                        }
                        break;
                    case Edge e:
                        myMesh.edges.Remove(e);
                        break;
                }
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
                    switch (primitive)
                    {
                        case Vertex v:
                            myMesh.vertices.Add(v);
                            break;
                        case Edge e:
                            myMesh.edges.Add(e);
                            break;
                    }
                }
                selectionManager.SetSelection(deleteAction.deletedPrimitives);
            }
            else
            {
                foreach (ISelectionPrimitive primitive in deleteAction.deletedPrimitives)
                {
                    //myMesh.vertices.Remove((SelectionManager.Vertex)primitive);
                    switch (primitive)
                    {
                        case Vertex v:
                            myMesh.vertices.Remove(v);
                            break;
                        case Edge e:
                            myMesh.edges.Remove(e);
                            break;
                    }
                }
                selectionManager.ClearSelection();
            }
        }
    }
}
