using UnityEngine;
using System.Collections.Generic;
using ISelectionPrimitive = SelectionManager.ISelectionPrimitive;
using SelectionMode = SelectionManager.SelectionMode;

/// <summary>
/// This file contains structures needed for the Undo / Redo stack
/// Technically these could also be used for some sort of automated testing / networking
/// </summary>
public interface IInputAction
{
    public void DebugPrint();
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

    public void DebugPrint()
    {
        Debug.Log("SelectAction: ");
        Debug.Log("Prev selection: ");
        SelectionManager.DebugPrimitiveList(prevSelection);
        Debug.Log("New selection: ");
        SelectionManager.DebugPrimitiveList(newSelection);
        Debug.Log("************");
    }
}

public class TranslateAction : IInputAction
{
    public Vector3 delta;

    public TranslateAction(Vector3 delta)
    {
        this.delta = delta;
    }
    
    public void DebugPrint()
    {
        Debug.Log("TranslateAction: ");
        Debug.Log("Delta: " + delta);
        Debug.Log("************");
    }
}

public class DeleteAction : IInputAction
{
    public List<ISelectionPrimitive> deletedPrimitives;
    
    public DeleteAction(List<ISelectionPrimitive> deletedPrimitives)
    {
        this.deletedPrimitives = deletedPrimitives;
    }
    
    public void DebugPrint()
    {
        Debug.Log("DeleteAction: ");
        Debug.Log("Deleted primitives: ");
        SelectionManager.DebugPrimitiveList(deletedPrimitives);
        Debug.Log("************");
    }
}

public class DuplicateAction : IInputAction
{
    public List<ISelectionPrimitive> duplicatedPrimitives;
    public List<ISelectionPrimitive> previousSelection;
    
    public DuplicateAction(List<ISelectionPrimitive> duplicatedPrimitives, List<ISelectionPrimitive> previousSelection)
    {
        this.duplicatedPrimitives = duplicatedPrimitives;
        this.previousSelection = previousSelection;
    }
    
    public void DebugPrint()
    {
        Debug.Log("DuplicateAction: ");
        Debug.Log("Duplicated primitives: ");
        SelectionManager.DebugPrimitiveList(duplicatedPrimitives);
        Debug.Log("Previous selection: ");
        SelectionManager.DebugPrimitiveList(previousSelection);
        Debug.Log("************");
    }
}

public class FormationAction : IInputAction
{
    public ISelectionPrimitive newPrimitive;
    
    public FormationAction(ISelectionPrimitive newPrimitive)
    {
        this.newPrimitive = newPrimitive;
    }
    
    public void DebugPrint()
    {
        Debug.Log("FormationAction: ");
        Debug.Log("New primitive: ");
        string foo = "";
        SelectionManager.DebugPrimitive(newPrimitive, ref foo);
        Debug.Log(foo);
        Debug.Log("************");
    }
}

public class ChangeModeAction : IInputAction
{
    public SelectionMode prevMode;
    public SelectionMode newMode;
    public SelectAction selectAction;
    
    public ChangeModeAction(SelectAction selectAction, 
        SelectionMode prevMode, SelectionMode newMode)
    {
        this.selectAction = selectAction;
        this.prevMode = prevMode;
        this.newMode = newMode;
    }
    
    public void DebugPrint()
    {
        Debug.Log("ChangeModeAction: ");
        Debug.Log("Prev mode: " + prevMode);
        Debug.Log("New mode: " + newMode);
        selectAction.DebugPrint();
        Debug.Log("************");
    }
}
