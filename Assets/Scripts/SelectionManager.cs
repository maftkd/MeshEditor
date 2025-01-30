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
        
        if (selectionDisabled)
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
                List<ISelectionPrimitive> newSelection = clickPhysics.FrustumOverlap(selectionBox);
                foreach(ISelectionPrimitive prim in newSelection)
                {
                    if (!_selection.Contains(prim))
                    {
                        Select(prim);
                    }
                }

                for (int i = _selection.Count - 1; i >= 0; i--)
                {
                    //only do this deselection thing for primitives in the current selection mode
                    // we have secondary prims like edges that get automatically selected via Select() or Deselect()
                    // that aren't directly affected by the box selection
                    if (!PrimMatchesMode(_selection[i]))
                    {
                        continue;
                    }
                    if (!newSelection.Contains(_selection[i]))
                    {
                        if(Input.GetKey(KeyCode.LeftShift) && _prevSelection.Contains(_selection[i]))
                        {
                            //don't deselect if it was part of the previous selection
                            continue;
                        }
                        Deselect(_selection[i]);
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
        switch (prim)
        {
            //when selecting a vertex, we also check to see if we have a full edge selected too
            case Vertex v:
                foreach (Edge e in mesh.edges)
                {
                    if (e.a == v || e.b == v)
                    {
                        if (!_selection.Contains(e))
                        {
                            if (_selection.Contains(e.a) && _selection.Contains(e.b))
                            {
                                Select(e);
                            }
                        }
                    }
                }
                break;
            case Edge e:
                if (!selection.Contains(e.a))
                {
                    Select(e.a);
                }

                if (!selection.Contains(e.b))
                {
                    Select(e.b);
                }
                break;
        }
        prim.selected = true;
    }
    
    public void Deselect(ISelectionPrimitive prim)
    {
        _selection.Remove(prim);
        switch (prim)
        {
            //when deselecting a vertex, we also check to see if we have broken any edges that should be deselected
            case Vertex v:
                foreach (Edge e in mesh.edges)
                {
                    if (e.a == v || e.b == v)
                    {
                        if (_selection.Contains(e))
                        {
                            if(!_selection.Contains(e.a) || !_selection.Contains(e.b))
                            {
                                Deselect(e);
                            }
                        }
                    }
                }
                break;
            case Edge e:
                break;
            
        }
        prim.selected = false;
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
            default:
                break;
        }
        SelectionChanged?.Invoke();
        selectionMode = newMode;
        modeText.text = $"Mode: {selectionMode}";
        UndoRedoStack.Instance.Push(new ChangeModeAction(new SelectAction(_prevSelection, _selection), prevMode, selectionMode));
        //ClearSelection();
    }
}
