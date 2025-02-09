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
                    List<ISelectionPrimitive> selectedEdges = selectionManager.GetSelectionByType(typeof(Edge));
                    List<ISelectionPrimitive> selectedVerts = selectionManager.GetSelectionByType(typeof(Vertex));
                    if (selectedEdges.Count >= 2)
                    {
                        List<ISelectionPrimitive> newlyFormedPrimitives = new List<ISelectionPrimitive>();
                        List<Loop> loops = new List<Loop>();
                        Edge first = selectedEdges[0] as Edge;
                        loops.Add(new Loop(first.a, first));
                        Vertex next = first.b;
                        selectedEdges.Remove(first);
                        while (loops.Count < selectedVerts.Count)
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
                                // we have at least one more selected edge but none are connected to the loop
                                // so we need to forge a connection here
                                // first we need to find the vertex that is closest to the current vertex
                                float minDistance = float.MaxValue;
                                Vertex closest = null;
                                foreach (Edge e in selectedEdges)
                                {
                                    bool loopContainsA = false;
                                    bool loopContainsB = false;
                                    foreach (Loop loop in loops)
                                    {
                                        if (loop.start == e.a)
                                        {
                                            loopContainsA = true;
                                        }
                                        if (loop.start == e.b)
                                        {
                                            loopContainsB = true;
                                        }
                                    }

                                    if (!loopContainsA)
                                    {
                                        float distance = Vector3.Distance(next.position, e.a.position);
                                        if (distance < minDistance)
                                        {
                                            minDistance = distance;
                                            closest = e.a;
                                        }
                                    }

                                    if (!loopContainsB)
                                    {
                                        float distance = Vector3.Distance(next.position, e.b.position);
                                        if (distance < minDistance)
                                        {
                                            minDistance = distance;
                                            closest = e.b;
                                        }
                                    }
                                }

                                if (closest == null)
                                {
                                    // this means we had no more selected edges, so it's time to close the loop
                                    nextEdge = new Edge(next, loops[0].start);
                                }
                                else
                                {
                                    nextEdge = new Edge(next, closest);
                                }
                                
                                mesh.edges.Add(nextEdge);
                                selectionManager.Select(nextEdge);
                                selectedEdges.Add(nextEdge);
                                newlyFormedPrimitives.Add(nextEdge);
                            }

                            loops.Add(new Loop(next, nextEdge));
                            selectedEdges.Remove(nextEdge);
                            next = nextEdge.a == next ? nextEdge.b : nextEdge.a;
                            if (next == loops[0].start)
                            {
                                //Debug.Log("Loop is closed");
                                break;
                            }
                        }
                        Debug.Log($"Finished loop without error: {loops.Count}");
                        int loopIndex = mesh.loops.Count;
                        foreach (Loop l in loops)
                        {
                            mesh.loops.Add(l);
                            newlyFormedPrimitives.Add(l);
                        }
                        Polygon newPoly = new Polygon(loopIndex, loops.Count);
                        PolygonHelper.CalculateNormal(newPoly, mesh);
                        PolygonHelper.Triangulate(newPoly, mesh);
                        mesh.polygons.Add(newPoly);
                        newlyFormedPrimitives.Add(newPoly);
                        UndoRedoStack.Instance.Push(new FormationAction(newlyFormedPrimitives));
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
                foreach(ISelectionPrimitive prim in formationAction.newPrimitives)
                {
                    selectionManager.Deselect(prim);
                    switch (prim)
                    {
                        case Edge e:
                            mesh.edges.Remove((Edge)prim);
                            break;
                        case Loop l:
                            mesh.loops.Remove((Loop)prim);
                            break;
                        case Polygon p:
                            mesh.polygons.Remove((Polygon)prim);
                            break;
                        default:
                            break;
                    }
                }
                /*
                selectionManager.Deselect(formationAction.newPrimitive);
                switch (formationAction.newPrimitive)
                {
                    case Edge e:
                        mesh.edges.Remove((Edge)formationAction.newPrimitive);
                        break;
                    default:
                        break;
                }
                */
            }
            else
            {
                foreach(ISelectionPrimitive prim in formationAction.newPrimitives)
                {
                    selectionManager.Select(prim);
                    switch (prim)
                    {
                        case Edge e:
                            mesh.edges.Add((Edge)prim);
                            break;
                        case Loop l:
                            mesh.loops.Add((Loop)prim);
                            break;
                        case Polygon p:
                            mesh.polygons.Add((Polygon)prim);
                            break;
                        default:
                            break;
                    }
                }
                /*
                selectionManager.Select(formationAction.newPrimitive);
                switch (formationAction.newPrimitive)
                {
                    case Edge e:
                        mesh.edges.Add((Edge)formationAction.newPrimitive);
                        break;
                    default:
                        break;
                }
                */
            }
        }
    }
}
