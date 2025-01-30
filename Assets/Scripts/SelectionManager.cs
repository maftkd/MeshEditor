using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Camera _cam;
    public ClickPhysics clickPhysics;
    public MyMesh mesh;
    
    // this is a class variable so we can maintain it over several update loops while box selecting
    private List<ISelectionPrimitive> _prevSelection = new();
    private List<ISelectionPrimitive> _selection = new();
    public List<ISelectionPrimitive> selection => _selection;
    public Action SelectionChanged;
    public bool selectionDisabled;
    public bool selectionDisabledViaCamera;
    public TextMeshProUGUI modeText;
    
    private Vector3 _boxStart;
    private Vector3 _boxEnd;
    
    public enum SelectionMode
    {
        Vertex,
        Edge,
        Face
    }

    public interface ISelectionPrimitive
    {
        public bool selected { get; set; }

        public ISelectionPrimitive Copy();
    }
    
    public class Vertex : ISelectionPrimitive
    {
        public Vector3 position;
        public bool selected { get; set; }

        public Vertex(Vector3 pos)
        {
            position = pos;
        }
        
        public ISelectionPrimitive Copy()
        {
            return new Vertex(position);
        }
    }
    
    public class Edge : ISelectionPrimitive
    {
        public Vertex a;
        public Vertex b;
        public bool selected { get; set; }

        public Edge(Vertex a, Vertex b)
        {
            this.a = a;
            this.b = b;
        }
        
        public ISelectionPrimitive Copy()
        {
            return new Edge(a, b);
        }

        public bool Contains(Vertex v)
        {
            return v==a || v==b;
        }
    }
    
    public SelectionMode selectionMode = SelectionMode.Vertex;
    
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        ChangeMode(SelectionMode.Vertex);
        UndoRedoStack.Instance.UndoRedo += OnUndoRedo;
        Shader.SetGlobalVector("_Box", Vector4.zero);
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
        SelectionMode newMode = selectionMode;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            newMode = SelectionMode.Vertex;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            newMode = SelectionMode.Edge;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            newMode = SelectionMode.Face;
        }

        if (newMode != selectionMode)
        {
            ChangeMode(newMode);
        }
        
        if (selectionDisabled || selectionDisabledViaCamera)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            
            //ignore any clicks that hit a Unity collider which we are using for gizmos
            if(Physics.Raycast(ray.origin, ray.direction))
            {
                return;
            }
            
            //track previous selection for the undo/redo stack
            _prevSelection = new List<ISelectionPrimitive>(_selection);
            
            //left shift is used for multi select, so when it's not pressed, we clear previous selection
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                ClearSelection();
            }
            
            ISelectionPrimitive hit = clickPhysics.Raycast(ray, selectionMode);
            if (hit != null)
            {
                if (!_selection.Contains(hit))
                {
                    Select(hit);
                }
            }
            
            SelectionChanged?.Invoke();
            
            _boxStart = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            // this means selection was disabled and we never properly received mouse down
            if(_boxStart == Vector3.zero)
            {
                return;
            }
            _boxEnd = Input.mousePosition;
            if ((_boxStart - _boxEnd).sqrMagnitude > 10)
            {
                float boxMinX = Mathf.Min(_boxStart.x, _boxEnd.x);
                float boxMaxX = Mathf.Max(_boxStart.x, _boxEnd.x);
                float boxMinY = Mathf.Min(_boxStart.y, _boxEnd.y);
                float boxMaxY = Mathf.Max(_boxStart.y, _boxEnd.y);
                //note y max, min are swapped to match the gpu's frag coords
                Shader.SetGlobalVector("_Box", new Vector4(boxMinX, Screen.height - boxMaxY, 
                    boxMaxX, Screen.height - boxMinY));
                
                Vector4 selectionBox = new Vector4(boxMinX / Screen.width, boxMinY / Screen.height, 
                    boxMaxX / Screen.width, boxMaxY / Screen.height);
                List<ISelectionPrimitive> newSelection = clickPhysics.FrustumOverlap(selectionBox, selectionMode);
                ClearSelection();
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (ISelectionPrimitive prim in _prevSelection)
                    {
                        Select(prim);
                    }
                }
                foreach(ISelectionPrimitive prim in newSelection)
                {
                    if (!_selection.Contains(prim))
                    {
                        Select(prim);
                    }
                }

                SelectionChanged?.Invoke();
                    
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // this means selection was disabled and we never properly received mouse down
            if(_boxStart == Vector3.zero)
            {
                return;
            }
            UndoRedoStack.Instance.Push(new SelectAction(_prevSelection, _selection));
            Shader.SetGlobalVector("_Box", Vector4.zero);
            
            //a marker that the click is complete - use this to avoid additional mouse events when selection is disabled
            _boxStart = Vector3.zero;
        }
    }

    public void Select(ISelectionPrimitive prim)
    {
        _selection.Add(prim);
        prim.selected = true;
        switch (prim)
        {
            case Vertex v:
                //in vertex selection mode, selecting vertices, sometimes auto-selects edges
                if (selectionMode == SelectionMode.Vertex)
                {
                    foreach (Edge e in mesh.edges)
                    {
                        if ((e.a == v || e.b == v) && !e.selected)
                        {
                            if (e.a.selected && e.b.selected)
                            {
                                Select(e);
                            }
                        }
                    }
                    
                }
                break;
            case Edge e:
                if (!e.a.selected)
                {
                    Select(e.a);
                }

                if (!e.b.selected)
                {
                    Select(e.b);
                }
                break;
        }
    }
    
    public void Deselect(ISelectionPrimitive prim)
    {
        _selection.Remove(prim);
        prim.selected = false;
        switch (prim)
        {
            case Vertex v:
                //in vertex selection mode, auto deselect edges when one of its vertices is deselected
                if (selectionMode == SelectionMode.Vertex)
                {
                    foreach (Edge e in mesh.edges)
                    {
                        if ((e.a == v || e.b == v) && e.selected)
                        {
                            if(!e.a.selected || !e.b.selected)
                            {
                                Deselect(e);
                            }
                        }
                    }
                }
                break;
            case Edge e:
                if (e.a.selected)
                {
                    Deselect(e.a);
                }
                if(e.b.selected)
                {
                    Deselect(e.b);
                }
                break;
            
        }
    }

    public void ClearSelection()
    {
        foreach (ISelectionPrimitive prim in _selection)
        {
            prim.selected = false;
        }
        _selection.Clear();
        SelectionChanged?.Invoke();
    }
    
    public void SetSelection(List<ISelectionPrimitive> newSelection)
    {
        ClearSelection();
        foreach (ISelectionPrimitive prim in newSelection)
        {
            Select(prim);
        }
        SelectionChanged?.Invoke();
    }
    
    void OnUndoRedo(IInputAction action, bool wasUndo)
    {
        if (action is SelectAction selectAction)
        {
            UndoRedoSelectAction(selectAction, wasUndo);
        }
        else if (action is ChangeModeAction changeModeAction)
        {
            //change mode
            if (wasUndo)
            {
                selectionMode = changeModeAction.prevMode;
            }
            else
            {
                selectionMode = changeModeAction.newMode;
            }
            modeText.text = $"Mode: {selectionMode}";
            
            //change selection
            UndoRedoSelectAction(changeModeAction.selectAction, wasUndo);
        }
    }

    void UndoRedoSelectAction(SelectAction selectAction, bool wasUndo)
    {
        if (wasUndo)
        {
            foreach (ISelectionPrimitive prim in selectAction.newSelection)
            {
                Deselect(prim);
            }
            
            foreach (ISelectionPrimitive prim in selectAction.prevSelection)
            {
                Select(prim);
            }
        }
        else
        {
            foreach (ISelectionPrimitive prim in selectAction.prevSelection)
            {
                Deselect(prim);
            }
            
            foreach (ISelectionPrimitive prim in selectAction.newSelection)
            {
                Select(prim);
            }
        }
        SelectionChanged?.Invoke();
    }
    
    bool PrimMatchesMode(ISelectionPrimitive prim)
    {
        switch (selectionMode)
        {
            case SelectionMode.Vertex:
                return prim is Vertex;
            case SelectionMode.Edge:
                return prim is Edge;
            case SelectionMode.Face:
                return false;
            default:
                return false;
        }
    }
    
    void ChangeMode(SelectionMode newMode)
    {
        SelectionMode prevMode = selectionMode;
        _prevSelection = new List<ISelectionPrimitive>(_selection);
        
        //leave old mode - deselect hanging elements that don't fit in new mode
        switch (prevMode)
        {
            case SelectionMode.Vertex:
                switch (newMode)
                {
                    case SelectionMode.Edge:
                        List<Vertex> includeVertices = new();
                        List<Vertex> discludeVertices = new();
                        
                        //find all vertices that are part of a selected edge
                        foreach (ISelectionPrimitive prim in selection)
                        {
                            if (prim is Edge e)
                            {
                                if (!includeVertices.Contains(e.a))
                                {
                                    includeVertices.Add(e.a);
                                }
                                if (!includeVertices.Contains(e.b))
                                {
                                    includeVertices.Add(e.b);
                                }
                            }
                        }
                        //find all vertices that are selected but not part of a selected edge
                        foreach (ISelectionPrimitive prim in selection)
                        {
                            if (prim is Vertex v)
                            {
                                if (!includeVertices.Contains(v))
                                {
                                    discludeVertices.Add(v);
                                }
                            }
                        }
                        //deselect all vertices that are not part of a selected edge
                        foreach (Vertex v in discludeVertices)
                        {
                            Deselect(v);
                        }
                        break;
                    default:
                        break;
                }
                break;
            case SelectionMode.Edge:
                switch (newMode)
                {
                    case SelectionMode.Vertex:
                        foreach (Edge e in mesh.edges)
                        {
                            if (!e.selected && e.a.selected && e.b.selected)
                            {
                                Select(e);
                            }
                        }
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        SelectionChanged?.Invoke();
        selectionMode = newMode;
        modeText.text = $"Mode: {selectionMode}";
        UndoRedoStack.Instance.Push(new ChangeModeAction(new SelectAction(_prevSelection, _selection), prevMode, selectionMode));
        //ClearSelection();
    }

    [ContextMenu("Debug Selection")]
    public void DebugSelection()
    {
        DebugPrimitiveList(selection);
    }

    public static void DebugPrimitiveList(List<ISelectionPrimitive> prims)
    {
        string dbg = "";
        foreach (ISelectionPrimitive prim in prims)
        {
            DebugPrimitive(prim, ref dbg);
        }
        Debug.Log(dbg);
    }

    public static void DebugPrimitive(ISelectionPrimitive prim, ref string dbg)
    {
        switch (prim)
        {
            case Vertex v:
                dbg += $"Vertex: {v.position}\n";
                break;
            case Edge e:
                dbg += $"Edge: {e.a.position} - {e.b.position}\n";
                break;
        }
    }
}
