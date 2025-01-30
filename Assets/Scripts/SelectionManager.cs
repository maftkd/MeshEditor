using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Camera _cam;
    public ClickPhysics clickPhysics;
    
    // this is a class variable so we can maintain it over several update loops while box selecting
    private List<ISelectionPrimitive> _prevSelection = new();
    private List<ISelectionPrimitive> _selection = new();
    public List<ISelectionPrimitive> selection => _selection;
    public Action SelectionChanged;
    public bool selectionDisabled;
    
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

    void Select(ISelectionPrimitive prim)
    {
        _selection.Add(prim);
        prim.selected = true;
    }
    
    void Deselect(ISelectionPrimitive prim)
    {
        _selection.Remove(prim);
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
        }
        SelectionChanged?.Invoke();
    }
}
