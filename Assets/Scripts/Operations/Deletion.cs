using System.Collections.Generic;
using UnityEngine;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using SelectionMode = SelectionManager.SelectionMode;
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
            List<ISelectionPrimitive> prevSelection = new List<ISelectionPrimitive>(selectionManager.selection);
            List<ISelectionPrimitive> deletedPrimitives = new();
            if (selectionManager.selectionMode == SelectionMode.Vertex)
            {
                foreach (ISelectionPrimitive prim in prevSelection)
                {
                    switch (prim)
                    {
                        case Vertex v:
                            deletedPrimitives.Add(v);
                            
                            //removing a vertex that belongs to an edge results in deletion of that edge too
                            foreach (Edge e in myMesh.edges)
                            {
                                if (e.Contains(v) && !deletedPrimitives.Contains(e))
                                {
                                    deletedPrimitives.Add(e);
                                }
                            }
                            break;
                        case Edge e:
                            if (!deletedPrimitives.Contains(e))
                            {
                                deletedPrimitives.Add(e);
                            }
                            //myMesh.edges.Remove(e);
                            break;
                    }
                }
            }
            else if (selectionManager.selectionMode == SelectionMode.Edge)
            {
                //remove edges
                foreach (ISelectionPrimitive prim in prevSelection)
                {
                    if (prim is Edge e)
                    {
                        deletedPrimitives.Add(e);
                    }
                }
                
                //also remove vertices associated with those edges that aren't shared with any other remaining edges
                foreach (Vertex v in myMesh.vertices)
                {
                    bool vertexShsaredWithRemainingEdge = false;
                    foreach (Edge e in myMesh.edges)
                    {
                        if (e.Contains(v) && !deletedPrimitives.Contains(e))
                        {
                            vertexShsaredWithRemainingEdge = true;
                            break;
                        }
                    }
                    if(!vertexShsaredWithRemainingEdge)
                    {
                        deletedPrimitives.Add(v);
                    }
                }
            }

            foreach (ISelectionPrimitive prim in deletedPrimitives)
            {
                switch (prim)
                {
                    case Vertex v:
                        myMesh.vertices.Remove(v);
                        break;
                    case Edge e:
                        myMesh.edges.Remove(e);
                        break;
                    default:
                        break;
                }
            }
            selectionManager.ClearSelection();
            UndoRedoStack.Instance.Push(new DeleteAction(deletedPrimitives, prevSelection));
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
                selectionManager.SetSelection(deleteAction.previousSelection);
            }
            else
            {
                foreach (ISelectionPrimitive primitive in deleteAction.deletedPrimitives)
                {
                    switch (primitive)
                    {
                        case Vertex v:
                            myMesh.vertices.Remove(v);
                            if (selectionManager.selectionMode == SelectionMode.Vertex)
                            {
                                selectionManager.Deselect(v);
                            }
                            break;
                        case Edge e:
                            myMesh.edges.Remove(e);
                            if (selectionManager.selectionMode == SelectionMode.Edge)
                            {
                                selectionManager.Deselect(e);
                            }
                            break;
                    }
                }
                selectionManager.ClearSelection();
            }
        }
    }
}
