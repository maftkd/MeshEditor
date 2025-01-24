using UnityEngine;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

public class Translation : MonoBehaviour
{
    public TransformGizmo gizmo;
    private GameObject gizmoGO;
    public SelectionManager selectionManager;

    private Vector3 _startPos;
    private Vector3 _currentPos;
    
    // Start is called before the first frame update
    void Start()
    {
        gizmoGO = gizmo.gameObject;
        gizmoGO.gameObject.SetActive(false);
        selectionManager.SelectionChanged += OnSelectionChanged;
        gizmo.Translated += HandleTranslationFromGizmo;
        gizmo.TranslationComplete += OnTranslationComplete;
        UndoRedoStack.Instance.UndoRedo += OnUndoRedo;
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
        }
    }
}
