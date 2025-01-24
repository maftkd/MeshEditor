using UnityEngine;
using System.Collections.Generic;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;

/// <summary>
/// This file contains structures needed for the Undo / Redo stack
/// Technically these could also be used for some sort of automated testing / networking
/// </summary>
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

public class TranslateAction : IInputAction
{
    public Vector3 delta;

    public TranslateAction(Vector3 delta)
    {
        this.delta = delta;
    }
}

public class DeleteAction : IInputAction
{
    public List<ISelectionPrimitive> deletedPrimitives;
    
    public DeleteAction(List<ISelectionPrimitive> deletedPrimitives)
    {
        this.deletedPrimitives = deletedPrimitives;
    }
}
