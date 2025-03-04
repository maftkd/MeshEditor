using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Loop = SelectionManager.Loop;
using Polygon = SelectionManager.Polygon;

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
            
            Dictionary<Vertex, Vertex> oldToNewVertices = new();
            Dictionary<Edge, Edge> oldToNewEdges = new();
            
            // verts
            foreach(ISelectionPrimitive prim in selectionManager.selection)
            {
                if (prim is Vertex v)
                {
                    Vertex newV = (Vertex)v.Copy();
                    _duplicatedPrimitives.Add(newV);
                    oldToNewVertices.Add(v, newV);
                }
            }
            
            //edges
            foreach (ISelectionPrimitive prim in selectionManager.selection)
            {
                if (prim is Edge e)
                {
                    Vertex newA = oldToNewVertices[e.a];
                    Vertex newB = oldToNewVertices[e.b];
                    Edge newEdge = new Edge(newA, newB);
                    _duplicatedPrimitives.Add(newEdge);
                    oldToNewEdges.Add(e, newEdge);
                }
            }
            
            //polys
            foreach (ISelectionPrimitive prim in selectionManager.selection)
            {
                if (prim is Polygon p)
                {
                    foreach(Loop l in myMesh.loops.GetRange(p.loopStartIndex, p.numLoops))
                    {
                        //assume the verts and edges have already been duplicated
                        Edge newEdge = oldToNewEdges[l.edge];
                        Vertex newVert = oldToNewVertices[l.start];
                        Loop newLoop = new Loop(newVert, newEdge);
                        _duplicatedPrimitives.Add(newLoop);
                    }
                    
                    Polygon poly = new Polygon(myMesh.loops.Count, p.numLoops);
                    _duplicatedPrimitives.Add(poly);
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
                    case Loop l:
                        myMesh.loops.Add(l);
                        break;
                    case Polygon p:
                        PolygonHelper.InitPoly(p, myMesh);
                        myMesh.polygons.Add(p);
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
                        case Loop l:
                            myMesh.loops.Remove(l);
                            break;
                        case Polygon p:
                            myMesh.polygons.Remove(p);
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
                        case Loop l:
                            myMesh.loops.Add(l);
                            break;
                        case Polygon p:
                            myMesh.polygons.Add(p);
                            break;
                    }
                }
                selectionManager.SetSelection(duplicateAction.duplicatedPrimitives);
            }
        }
    }
}
