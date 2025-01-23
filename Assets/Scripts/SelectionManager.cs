using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Camera _cam;
    public ClickPhysics clickPhysics;
    private List<ISelectionPrimitive> _selection = new();
    
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            
            ISelectionPrimitive hit = clickPhysics.Raycast(ray, selectionMode);
            //hit = raycaster.Raycast(ray, selectionMode)
            if (hit != null)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    ClearSelection();
                }
                _selection.Add(hit);
                hit.selected = true;
            }
            else
            {
                ClearSelection();
            }
        }
    }

    void ClearSelection()
    {
        foreach (ISelectionPrimitive prim in _selection)
        {
            prim.selected = false;
        }
        _selection.Clear();
    }
}
