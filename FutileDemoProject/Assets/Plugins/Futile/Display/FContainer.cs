using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FContainer : FNode
{
	protected List<FNode> _childNodes = new List<FNode>(5);
	
	private int _oldChildNodesHash = 0;
	
	private bool _shouldSortByZ = false; //don't turn this on unless you really need it, it'll do a sort every redraw
	
	public FContainer () : base()
	{
		
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		int childCount = _childNodes.Count;
		for(int c = 0; c<childCount; c++)
		{
			_childNodes[c].Redraw(shouldForceDirty || wasMatrixDirty || wasAlphaDirty, shouldUpdateDepth); //if the matrix was dirty or we're supposed to force it, do it!
		}
	}
	
	override public void HandleAddedToStage()
	{
		if(!_isOnStage)
		{
			base.HandleAddedToStage();
			
			int childCount = _childNodes.Count;
			for(int c = 0; c<childCount; c++)
			{
				FNode childNode = _childNodes[c];
				childNode.stage = _stage;
				childNode.HandleAddedToStage();	
			}
			
			if(_shouldSortByZ) 
			{
				Futile.instance.SignalUpdate += HandleUpdateAndSort;
			}
		}
		
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			base.HandleRemovedFromStage();
			
			int childCount = _childNodes.Count;
			for(int c = 0; c<childCount; c++)
			{
				FNode childNode = _childNodes[c];
				childNode.HandleRemovedFromStage();	
				childNode.stage = null;
			}
			
			if(_shouldSortByZ)
			{
				Futile.instance.SignalUpdate -= HandleUpdateAndSort;
			}
		}
	}
	
	private void HandleUpdateAndSort()
	{
		bool didChildOrderChangeAfterSort = SortByZ();
		
		if(didChildOrderChangeAfterSort) //sort the order, and then if the child order was changed, repopulate the renderlayer
		{
			if(_isOnStage) _stage.HandleFacetsChanged();	
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
				node.stage = _stage;
				node.HandleAddedToStage();
			}
		}
		else if(nodeIndex != _childNodes.Count-1) //if node is already a child, put it at the top of the children if it's not already
		{
			_childNodes.RemoveAt(nodeIndex);
			_childNodes.Add(node);
			if(_isOnStage) _stage.HandleFacetsChanged(); 
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
				node.stage = _stage;
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
			
			if(_isOnStage) _stage.HandleFacetsChanged();
		}
	}
	
	public void RemoveChild(FNode node)
	{
		if(node.container != this) return; //I ain't your daddy
		
		node.HandleRemovedFromContainer();
		
		if(_isOnStage)
		{
			node.HandleRemovedFromStage();
			node.stage = null;
		}
		
		_childNodes.Remove(node);
	}
	
	public void RemoveAllChildren()
	{
		int childCount = _childNodes.Count;
		
		for(int c = 0; c<childCount; c++)
		{
			FNode node = _childNodes[c];
			
			node.HandleRemovedFromContainer();
			
			if(_isOnStage)
			{
				node.HandleRemovedFromStage();
				node.stage = null;
			}
		}
	
		_childNodes.Clear();	
	}
	
	public int GetChildCount()
	{
		return _childNodes.Count;
	}
	
	public FNode GetChildAt(int childIndex)
	{
		return _childNodes[childIndex];
	}
	
	public bool shouldSortByZ
	{
		get {return _shouldSortByZ;}
		set 
		{
			if(_shouldSortByZ != value)
			{
				_shouldSortByZ = value;
				
				if(_shouldSortByZ)
				{
					if(_isOnStage) 
					{
						Futile.instance.SignalUpdate += HandleUpdateAndSort;
					}
				}
				else 
				{
					if(_isOnStage) 
					{
						Futile.instance.SignalUpdate -= HandleUpdateAndSort;
					}
				}
			}
		}
	}
	
	private static int ZComparison(FNode a, FNode b) 
	{
		float delta = a.sortZ - b.sortZ;
		if(delta < 0) return -1;
		if(delta > 0) return 1;
		return 0;
	}
	
	private bool SortByZ() //returns true if the childNodes changed, false if they didn't
	{
		//using InsertionSort because it's stable (meaning equal values keep the same order)
		//this is unlike List.Sort, which is unstable, so things would constantly shift if equal.
		_childNodes.InsertionSort(ZComparison);
		
		//check if the order has changed, and if it has, update the quads/depth order
		//http://stackoverflow.com/questions/3030759/arrays-lists-and-computing-hashvalues-vb-c
		
		int hash = 269;
		
		unchecked //don't throw int overflow exceptions
		{
			int childCount = _childNodes.Count;
			for(int c = 0; c<childCount; c++)
			{
				hash = (hash * 17) + _childNodes[c].GetHashCode();
			}
		} 
		
		if(hash != _oldChildNodesHash)
		{
			_oldChildNodesHash = hash;
			return true; //order has changed
		}
		
		return false; //order hasn't changed
	}
}

