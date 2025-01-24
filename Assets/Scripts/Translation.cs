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

    private bool _hotkeyTranslation;
    private Vector3 _offset;
    private Camera _cam;
    private Vector3 _axesConstraints = Vector3.one;
    
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
        // If we have an active selection, listen for G hotkey as an alternate form of translation
        // Also check that we aren't already dragging with th emouse
        if (_selectionReadyToTranslate && !Input.GetMouseButton(0))
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (ClickPhysics.RaycastMouseToPlaneAtPoint(_startPos, _cam, out Vector3 hitPos))
                {
                    _offset = hitPos - _startPos;
                    SetHotkeyTranslation(true);
                    _currentPos = _startPos;
                }
            }
        }
        
        if (_hotkeyTranslation)
        {
            
            
            // Axes constraints
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _axesConstraints = Vector3.one - Vector3.right;
                }
                else
                {
                    _axesConstraints = Vector3.right;
                }
                SetLineGizmoVisibility();
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _axesConstraints = Vector3.one - Vector3.up;
                }
                else
                {
                    _axesConstraints = Vector3.up;
                }
                SetLineGizmoVisibility();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    _axesConstraints = Vector3.one - Vector3.forward;
                }
                else
                {
                    _axesConstraints = Vector3.forward;
                }
                SetLineGizmoVisibility();
            }
            
            
            
            // Determine where mouse ray falls on translation plane
            if(ClickPhysics.RaycastMouseToPlaneAtPoint(_currentPos, _cam, out Vector3 hitPos))
            {
                // some wackiness here for axis constraints
                Vector3 tmpConstraint = _axesConstraints;
                
                // Essentially we first reset the selection to the start position with no constraints
                _axesConstraints = Vector3.one;
                HandleTranslationFromGizmo(_startPos);
                
                // Then we calculate the offset from the start position to the hit position including constraints
                _axesConstraints = tmpConstraint;
                HandleTranslationFromGizmo(hitPos - _offset);
                
                gizmoGO.transform.position = _currentPos;
            }

            
            
            // lmb to Confirm translation
            if (Input.GetMouseButtonDown(0))
            {
                OnTranslationComplete();
                SetHotkeyTranslation(false);
            }
            
            // rmb to cancel translation
            else if (Input.GetMouseButtonDown(1))
            {
                _axesConstraints = Vector3.one;
                HandleTranslationFromGizmo(_startPos);
                SetHotkeyTranslation(false);
                gizmoGO.transform.position = _startPos;
            }
            
            
        }
    }

    void SetHotkeyTranslation(bool translationEnabled)
    {
        //toggle state flag
        _hotkeyTranslation = translationEnabled;
        
        //prevent finalization click from getting handled by selection manager
        selectionManager.selectionDisabled = translationEnabled;
        
        //reset axis constraints - doing this at the beginning and end is a bit redundant, but it works
        _axesConstraints = Vector3.one;
        
        //gizmo viz - main gizmo hidden while hotkey translating. Lines hidden at start and end 
        SetGizmoVisibility(!translationEnabled);
        SetLineGizmoVisibility();
    }

    void HandleTranslationFromGizmo(Vector3 pos)
    {
        Vector3 delta = pos - _currentPos;
        delta.x *= _axesConstraints.x;
        delta.y *= _axesConstraints.y;
        delta.z *= _axesConstraints.z;
        foreach (ISelectionPrimitive prim in selectionManager.selection)
        {
            SelectionManager.Vertex vertex = (SelectionManager.Vertex) prim;
            vertex.position += delta;
        }
        _currentPos += delta;
    }
    
    void OnTranslationComplete()
    {
        UndoRedoStack.Instance.Push(new TranslateAction(_currentPos - _startPos));
        _startPos = _currentPos;
        gizmoGO.transform.position = _currentPos;
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
    
    void SetLineGizmoVisibility()
    {
        if (_axesConstraints == Vector3.one)
        {
            foreach(GameObject line in lines)
            {
                line.SetActive(false);
            }
            return;
        }
        
        lines[0].SetActive(_axesConstraints.x > 0.5f);
        lines[1].SetActive(_axesConstraints.y > 0.5f);
        lines[2].SetActive(_axesConstraints.z > 0.5f);
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
            _startPos = gizmoGO.transform.position;
        }
    }
}
