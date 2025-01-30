using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;

public class Duplication : MonoBehaviour
{
    public SelectionManager selectionManager;
    public MyMesh myMesh;
    public UnityEvent OnDuplicate;
    private List<ISelectionPrimitive> _previousSelection = new();
    private List<ISelectionPrimitive> _duplicatedPrimitives = new();
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
            _duplicatedPrimitives = new List<ISelectionPrimitive>();
            _previousSelection = new List<ISelectionPrimitive>(selectionManager.selection);
            
            //first go through higher-order prims
            foreach (ISelectionPrimitive prim in selectionManager.selection)
            {
                if (prim is Edge e)
                {
                    Vertex newA = (Vertex)e.a.Copy();
                    Vertex newB = (Vertex)e.b.Copy();
                    Edge newEdge = new Edge(newA, newB);
                    _duplicatedPrimitives.Add(newEdge);
                    _duplicatedPrimitives.Add(newA);
                    _duplicatedPrimitives.Add(newB);
                }
            }
            // then lower order
            foreach(ISelectionPrimitive prim in selectionManager.selection)
            {
                if (prim is Vertex v)
                {
                    Vertex newV = (Vertex)v.Copy();
                    _duplicatedPrimitives.Add(newV);
                }
            }

            foreach (ISelectionPrimitive prim in _duplicatedPrimitives)
            {
                switch (prim)
                {
                    case Vertex v:
                        myMesh.vertices.Add(v);
                        break;
                    case Edge e:
                        myMesh.edges.Add(e);
                        break;
                }
            }
            
            selectionManager.SetSelection(_duplicatedPrimitives);
            
            //note this event triggers translation, which in turn triggers undo/redo
            OnDuplicate?.Invoke();
        }
    }

    public void DuplicationTranslationComplete(bool wasDuplicate)
    {
        if (!wasDuplicate)
        {
            return;
        }
        
        UndoRedoStack.Instance.Push(new DuplicateAction(_duplicatedPrimitives, _previousSelection));
    }
    
    void OnUndoRedo(IInputAction action, bool isUndo)
    {
        if (action is DuplicateAction duplicateAction)
        {
            if (isUndo)
            {
                foreach (var primitive in duplicateAction.duplicatedPrimitives)
                {
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
                selectionManager.SetSelection(duplicateAction.previousSelection);
            }
            else
            {
                foreach (var primitive in duplicateAction.duplicatedPrimitives)
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
                selectionManager.SetSelection(duplicateAction.duplicatedPrimitives);
            }
        }
    }
}
