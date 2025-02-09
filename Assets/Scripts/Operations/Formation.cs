using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SelectionMode = SelectionManager.SelectionMode;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using Vertex = SelectionManager.Vertex;
using Edge = SelectionManager.Edge;
using Loop = SelectionManager.Loop;
using Polygon = SelectionManager.Polygon;

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
                case SelectionMode.Edge:
                    // tmp only work if 4 edges are selected
                    // ideally later we can do something more generic??
                    // I think the thing to do to make it generic is to check at the end if the last vertex = the start vertex
                    // if not, then we will need to plop in a new edge
                    List<ISelectionPrimitive> selectedEdges = selectionManager.GetSelectionByType(typeof(Edge));
                    if (selectedEdges.Count == 4)
                    {
                        List<Loop> loops = new List<Loop>();
                        Edge first = selectedEdges[0] as Edge;
                        loops.Add(new Loop(first.a, first));
                        Vertex next = first.b;
                        selectedEdges.Remove(first);
                        while (selectedEdges.Count > 0)
                        {
                            Edge nextEdge = null;
                            foreach (Edge e in selectedEdges)
                            {
                                if (e.a == next || e.b == next)
                                {
                                    nextEdge = e;
                                    break;
                                }
                            }
                            if (nextEdge == null)
                            {
                                Debug.LogError("Invalid edge selection");
                                break;
                            }
                            loops.Add(new Loop(next, nextEdge));
                            selectedEdges.Remove(nextEdge);
                            next = nextEdge.a == next ? nextEdge.b : nextEdge.a;
                            if (next == loops[0].start)
                            {
                                break;
                            }
                        }
                        Debug.Log($"Finished loop without error: {loops.Count}");
                        int loopIndex = mesh.loops.Count;
                        foreach (Loop l in loops)
                        {
                            mesh.loops.Add(l);
                        }
                        Polygon newPoly = new Polygon(loopIndex, loops.Count);
                        PolygonHelper.CalculateNormal(newPoly, mesh);
                        PolygonHelper.Triangulate(newPoly, mesh);
                        mesh.polygons.Add(newPoly);
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
