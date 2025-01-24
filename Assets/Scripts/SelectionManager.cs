using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Camera _cam;
    public ClickPhysics clickPhysics;
    private List<ISelectionPrimitive> _selection = new();
    public List<ISelectionPrimitive> selection => _selection;
    public Action SelectionChanged;
    
    public enum SelectionMode
    {
        Vertex,
        Edge,
        Face
    }

    public interface ISelectionPrimitive
    {
        public bool selected { get; set; }

    }
    
    public class Vertex : ISelectionPrimitive
    {
        public Vector3 position;
        public bool selected { get; set; }

        public Vertex(Vector3 pos)
        {
            position = pos;
        }
    }
    
    public SelectionMode selectionMode = SelectionMode.Vertex;
    
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            
            //ignore any clicks that hit a Unity collider which we are using for gizmos
            if(Physics.Raycast(ray.origin, ray.direction))
            {
                return;
            }
            
            //track previous selection for the undo/redo stack
            List<ISelectionPrimitive> prevSelection = new List<ISelectionPrimitive>(_selection);
            
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
            
            UndoRedoStack.Instance.PushAction(new SelectAction(prevSelection, _selection));
            
            SelectionChanged?.Invoke();
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

    void ClearSelection()
    {
        foreach (ISelectionPrimitive prim in _selection)
        {
            prim.selected = false;
        }
        _selection.Clear();
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
