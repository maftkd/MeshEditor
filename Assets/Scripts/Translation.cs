using System.Linq;
using UnityEngine;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

public class Translation : MonoBehaviour
{
    public TransformGizmo gizmo;
    private GameObject gizmoGO;
    public GameObject[] lines;
    public SelectionManager selectionManager;

    private Vector3 _startPos;
    private Vector3 _currentPos;

    private bool _translatingViaHotkey;
    private Vector3 _offset;
    private Camera _cam;
    
    //using the gizmo as a hint that the selection is ready to be translated, since its updated on selection change
    private bool _selectionReadyToTranslate => gizmoGO.activeSelf;
    
    // Start is called before the first frame update
    void Start()
    {
        gizmoGO = gizmo.gameObject;
        gizmoGO.gameObject.SetActive(false);
        
        selectionManager.SelectionChanged += OnSelectionChanged;
        gizmo.Translated += HandleTranslationFromGizmo;
        gizmo.TranslationComplete += OnTranslationComplete;
        UndoRedoStack.Instance.UndoRedo += OnUndoRedo;
        
        _cam = Camera.main;
    }

    private void OnDestroy()
    {
        selectionManager.SelectionChanged -= OnSelectionChanged;
        gizmo.Translated -= HandleTranslationFromGizmo;
        gizmo.TranslationComplete -= OnTranslationComplete;
        if (UndoRedoStack.Instance != null)
        {
            UndoRedoStack.Instance.UndoRedo -= OnUndoRedo;
        }
    }

    // position the gizmo and set visibility based on selection
    void OnSelectionChanged()
    {
        if (selectionManager.selection.Count == 0)
        {
            gizmoGO.SetActive(false);
        }
        else if(selectionManager.selection.Count == 1)
        {
            gizmoGO.SetActive(true);
            gizmoGO.transform.position = ((SelectionManager.Vertex)selectionManager.selection[0]).position;
        }
        else
        {
            Vector3 average = Vector3.zero;
            foreach (var selection in selectionManager.selection)
            {
                average += ((SelectionManager.Vertex)selection).position;
            }
            
            average /= selectionManager.selection.Count;
            gizmoGO.SetActive(true);
            gizmoGO.transform.position = average;
        }

        _startPos = gizmoGO.transform.position;
        _currentPos = _startPos;
    }

    void Update()
    {
        //this is like Blender's translate with the "G" key
        if (_translatingViaHotkey)
        {
            // Determine where mouse ray falls on translation plane
            if(ClickPhysics.RaycastMouseToPlaneAtPoint(_currentPos, _cam, out Vector3 hitPos))
            {
                HandleTranslationFromGizmo(hitPos - _offset);
                gizmoGO.transform.position = _currentPos;
            }

            // lmb to Confirm translation
            if (Input.GetMouseButtonDown(0))
            {
                OnTranslationComplete();
                SetTranslationModeViaHotkey(false);
            }
            
            // rmb to cancel translation
            else if (Input.GetMouseButtonDown(1))
            {
                HandleTranslationFromGizmo(_startPos);
                SetTranslationModeViaHotkey(false);
            }
        }
        
        // If we have an active selection, listen for G hotkey as an alternate form of translation
        if (_selectionReadyToTranslate)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (ClickPhysics.RaycastMouseToPlaneAtPoint(_startPos, _cam, out Vector3 hitPos))
                {
                    _offset = hitPos - _startPos;
                    SetTranslationModeViaHotkey(true);
                }
            }
        }
    }

    void SetTranslationModeViaHotkey(bool translationEnabled)
    {
        _translatingViaHotkey = translationEnabled;
        SetGizmoVisibility(!translationEnabled);
        selectionManager.selectionDisabled = translationEnabled;
        gizmoGO.transform.position = _currentPos;
        _startPos = _currentPos;
    }

    void HandleTranslationFromGizmo(Vector3 pos)
    {
        Vector3 delta = pos - _currentPos;
        foreach (ISelectionPrimitive prim in selectionManager.selection)
        {
            SelectionManager.Vertex vertex = (SelectionManager.Vertex) prim;
            vertex.position += delta;
        }
        _currentPos = pos;
    }
    
    void OnTranslationComplete()
    {
        UndoRedoStack.Instance.PushAction(new TranslateAction(_currentPos - _startPos));
        _startPos = _currentPos;
    }

    void SetGizmoVisibility(bool visible)
    {
        foreach (Transform t in gizmoGO.transform)
        {
            if (lines.Contains(t.gameObject))
            {
                continue;
            }
            t.gameObject.SetActive(visible);
        }
    }
    
    void OnUndoRedo(IInputAction action, bool wasUndo)
    {
        if (action is TranslateAction translateAction)
        {
            float sign = wasUndo ? -1f : 1f;
            Vector3 delta = translateAction.delta;
            foreach (ISelectionPrimitive prim in selectionManager.selection)
            {
                SelectionManager.Vertex vertex = (SelectionManager.Vertex) prim;
                vertex.position += delta * sign;
            }
            gizmoGO.transform.position += delta * sign;
        }
    }
}
