using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;








public class FScrollTouchManager : FTouchManager
{
	protected bool _isScrolling=false;

	public FScrollTouchManager () : base()
	{
	}
	
	override public void Update()
	{
		if (_isScrolling) return;
		
		base.Update();
	}
	
	virtual public bool isScrolling 
	{
		get 
		{
			return _isScrolling;
		}
		set 
		{
			if (value!=_isScrolling) {
				_isScrolling=value;
				if(_theSingleTouchable != null)
				{
					_theSingleTouchable.HandleSingleTouchCanceled(new FTouch());
					_theSingleTouchable=null;
				}
			}
		}
	}
}

