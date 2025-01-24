using System.Collections.Generic;
using UnityEngine;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

public class InputAction
{
}

public interface IInputAction
{
}

public class SelectAction : IInputAction
{
    public List<ISelectionPrimitive> prevSelection;
    public List<ISelectionPrimitive> newSelection;
    
    public SelectAction(List<ISelectionPrimitive> prevSelection, List<ISelectionPrimitive> newSelection)
    {
        this.prevSelection = new List<ISelectionPrimitive>(prevSelection);
        this.newSelection = new List<ISelectionPrimitive>(newSelection);
    }
}
