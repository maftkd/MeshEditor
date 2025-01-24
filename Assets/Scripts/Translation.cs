using UnityEngine;

public class Translation : MonoBehaviour
{
    public TransformGizmo gizmo;
    private GameObject gizmoGO;

    public SelectionManager selectionManager;
    
    // Start is called before the first frame update
    void Start()
    {
        gizmoGO = gizmo.gameObject;
        gizmoGO.gameObject.SetActive(false);
        selectionManager.SelectionChanged += OnSelectionChanged;
        gizmo.OnTranslated += HandleTranslationFromGizmo;
    }

    private void OnDestroy()
    {
        selectionManager.SelectionChanged -= OnSelectionChanged;
        gizmo.OnTranslated -= HandleTranslationFromGizmo;
    }

    void OnSelectionChanged()
    {
        if (selectionManager._selection.Count == 0)
        {
            gizmoGO.SetActive(false);
        }
        else if(selectionManager._selection.Count == 1)
        {
            gizmoGO.SetActive(true);
            gizmoGO.transform.position = ((SelectionManager.Vertex)selectionManager._selection[0]).position;
        }
        else
        {
            Vector3 average = Vector3.zero;
            foreach (var selection in selectionManager._selection)
            {
                average += ((SelectionManager.Vertex)selection).position;
            }
            
            average /= selectionManager._selection.Count;
            gizmoGO.SetActive(true);
            gizmoGO.transform.position = average;
        }
    }

    void HandleTranslationFromGizmo(Vector3 pos)
    {
        Debug.Log("Translated: " + pos);
        
    }
}
