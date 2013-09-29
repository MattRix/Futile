using System;
using UnityEngine;

public class FNodeEnablerForScrollSingleTouch : FNodeEnabler
{
	protected FNode _node=null;
	protected FTouchManager _touchManager=null;
	public FNodeEnablerForScrollSingleTouch(FNode node)
	{
		FSingleTouchableInterface singleTouchable = node as FSingleTouchableInterface;
		if(singleTouchable == null)
		{
			throw new FutileException("Trying to enable scroll single touch on a node that doesn't implement FSingleTouchableInterface");	
		}
		_node = node;
	}
	
	override public void Connect()
	{
		_touchManager=null;
		FContainer parent=_node.container;
		//Find a FScrollContainer in the tree of upper containers, if so subscribe to its touch manager, otherwise subscribe to futile's touch manager
		while (parent!=null) {
			FScrollContainer scrollContainer=parent as FScrollContainer;
			if (scrollContainer!=null) {
				_touchManager=scrollContainer.touchManager;
				//Debug.Log ("FScrollContainer found");
				break;
			}
			parent=parent.container;
		}
		if (_touchManager==null) {
			_touchManager=Futile.touchManager;
		}
		_touchManager.AddSingleTouchTarget(_node as FSingleTouchableInterface);	
	}
	
	override public void Disconnect()
	{
		if (_touchManager!=null) _touchManager.RemoveSingleTouchTarget(_node as FSingleTouchableInterface);	
	}
}

