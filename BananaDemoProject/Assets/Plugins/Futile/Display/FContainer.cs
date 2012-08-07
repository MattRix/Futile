using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FContainer : FNode
{
	protected List<FNode> _childNodes = new List<FNode>();
	
	public FContainer ()
	{
		
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		base.Redraw(shouldForceDirty, shouldUpdateDepth);
		
		foreach(FNode node in _childNodes)
		{
			node.Redraw(shouldForceDirty || wasMatrixDirty || wasAlphaDirty, shouldUpdateDepth); //if the matrix was dirty or we're supposed to force it, do it!
		}
	}
	
	override public void HandleAddedToStage()
	{
		if(!_isOnStage)
		{
			_isOnStage = true;
			
			foreach(FNode childNode in _childNodes)
			{
				childNode.HandleAddedToStage();	
			}
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			_isOnStage = false;
			
			foreach(FNode childNode in _childNodes)
			{
				childNode.HandleRemovedFromStage();	
			}
		}
	}
	
	public void AddChild(FNode node)
	{
		int nodeIndex = _childNodes.IndexOf(node);
		
		if(nodeIndex == -1) //add it if it's not a child
		{
			node.HandleAddedToContainer(this);
			_childNodes.Add(node);
			
			if(_isOnStage)
			{
				node.HandleAddedToStage();
			}
		}
		else if(nodeIndex != _childNodes.Count-1) //if node is already a child, put it at the top of the children if it's not already
		{
			_childNodes.RemoveAt(nodeIndex);
			_childNodes.Add(node);
			if(_isOnStage) _stage.HandleQuadsChanged(); 
		}
	}
	
	public void AddChildAtIndex(FNode node, int newIndex)
	{
		int nodeIndex = _childNodes.IndexOf(node);
		
		if(newIndex > _childNodes.Count) //if it's past the end, make it at the end
		{
			newIndex = _childNodes.Count;
		}
		
		if(nodeIndex == newIndex) return; //if it's already at the right index, just leave it there
		
		if(nodeIndex == -1) //add it if it's not a child
		{
			node.HandleAddedToContainer(this);
			
			_childNodes.Insert(newIndex, node);
			
			if(_isOnStage)
			{
				node.HandleAddedToStage();
			}
		}
		else //if node is already a child, move it to the desired index
		{
			_childNodes.RemoveAt(nodeIndex);
			
			if(nodeIndex < newIndex)
			{
				_childNodes.Insert(newIndex-1, node); //gotta subtract 1 to account for it moving in the order
			}
			else
			{
				_childNodes.Insert(newIndex, node);
			}
			
			if(_isOnStage) _stage.HandleQuadsChanged();
		}
	}
	
	public void RemoveChild(FNode node)
	{
		node.HandleRemovedFromContainer();
		
		if(_isOnStage)
		{
			node.HandleRemovedFromStage();
		}
		
		_childNodes.Remove(node);
	}
	
	public int GetChildCount()
	{
		return _childNodes.Count;
	}
	
	public FNode GetChildAt(int childIndex)
	{
		return _childNodes[childIndex];
	}
}

