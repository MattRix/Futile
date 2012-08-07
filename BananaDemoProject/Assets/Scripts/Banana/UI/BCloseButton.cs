using UnityEngine;
using System.Collections;
using System;

public class BCloseButton : FContainer, FSingleTouchableInterface
{
	protected FAtlasElement _normalElement;
	protected FAtlasElement _overElement;
	protected FSprite _bg;
	
	public event EventHandler SignalTap;
	
	public BCloseButton ()
	{
		_normalElement = Futile.atlasManager.GetElementWithName("CloseButton_normal.png");
		_overElement = Futile.atlasManager.GetElementWithName("CloseButton_over.png");
		
		_bg = new FSprite(_normalElement.name);
		AddChild(_bg);
	}
	
	override public void HandleAddedToStage()
	{
		base.HandleAddedToStage();	
		Futile.touchManager.AddSingleTouchTarget(this);
	}
	
	override public void HandleRemovedFromStage()
	{
		base.HandleRemovedFromStage();	
		Futile.touchManager.RemoveSingleTouchTarget(this);
	}
	
	public bool HandleSingleTouchBegan(FTouch touch)
	{
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.boundsRect.Contains(touchPos))
		{
			_bg.element = _overElement;
			return true;	
		}
		
		return false;
	}
	
	public void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.boundsRect.Contains(touchPos))
		{
			_bg.element = _overElement;	
		}
		else
		{
			_bg.element =_normalElement;	
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_bg.element = _normalElement;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.boundsRect.Contains(touchPos))
		{
			BSoundPlayer.PlayClickSound();
			if(SignalTap != null) SignalTap(this, EventArgs.Empty);
		}
	}
	
	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_bg.element = _normalElement;
	}
	
	
}


