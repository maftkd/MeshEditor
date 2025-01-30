using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SelectionMode = SelectionManager.SelectionMode;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;

public class Formation : MonoBehaviour
{
    public SelectionManager selectionManager;

    public MyMesh mesh;
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            switch (selectionManager.selectionMode)
            {
                case SelectionMode.Vertex:
                    if (selectionManager.selection.Count == 2)
                    {
                        if (selectionManager.selection[0] is Vertex a &&
                            selectionManager.selection[1] is Vertex b)
                        {
                            Edge newEdge = new Edge(a, b);
                            mesh.edges.Add(newEdge);
                            selectionManager.Select(newEdge);
                            UndoRedoStack.Instance.Push(new FormationAction(newEdge));
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
    
    void OnUndoRedo(IInputAction action, bool isUndo)
    {
        if(action is FormationAction formationAction)
        {
            if (isUndo)
            {
                selectionManager.Deselect(formationAction.newPrimitive);
                switch (formationAction.newPrimitive)
                {
                    case Edge e:
                        mesh.edges.Remove((Edge)formationAction.newPrimitive);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                selectionManager.Select(formationAction.newPrimitive);
                switch (formationAction.newPrimitive)
                {
                    case Edge e:
                        mesh.edges.Add((Edge)formationAction.newPrimitive);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
