using UnityEngine;
using System.Collections;
using System;

public class FButton : FContainer, FSingleTouchable
{
	protected FAtlasElement _normalElement;
	protected FAtlasElement _pressedElement;
	protected FSprite _bg;
	protected string _sound;
	protected FLabel _label;

	public event EventHandler SignalPress;
	public event EventHandler SignalRelease;
	
	public FButton (string upImage, string downImage, string sound)
	{
		_normalElement = FEngine.atlasManager.GetElementWithName(upImage);
		_pressedElement = FEngine.atlasManager.GetElementWithName(downImage);
		_bg = new FSprite(_normalElement.name);
		AddChild(_bg);

		_sound = sound;
	}
	// Simpler constructors
	public FButton (string upImage, string downImage) : this(upImage, downImage, null) {}
	public FButton (string upImage) : this(upImage, upImage, null) {}

	public void AddLabel (string fontName, string text, Color color)
	{
		if(_label != null) 
		{
			RemoveChild(_label);
		}

		_label = new FLabel(fontName, text);
		AddChild(_label);
		_label.color = color;
		_label.anchorY = 0.0f;
		_label.y = 0;
	}

	public FLabel label
	{
		get {return _label;}
	}

	override public void HandleAddedToStage()
	{
		base.HandleAddedToStage();	
		FEngine.touchManager.AddSingleTouchTarget(this);
	}
	
	override public void HandleRemovedFromStage()
	{
		base.HandleRemovedFromStage();	
		FEngine.touchManager.RemoveSingleTouchTarget(this);
	}
	
	public bool HandleSingleTouchBegan(FTouch touch)
	{
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.localRect.Contains(touchPos))
		{
			_bg.element = _pressedElement;
			
			if(_sound != null) FSoundManager.PlaySound(_sound);
			
			if(SignalPress != null) SignalPress(this, EventArgs.Empty);
			
			return true;	
		}
		
		return false;
	}
	
	public void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.localRect.Contains(touchPos))
		{
			_bg.element = _pressedElement;	
		}
		else
		{
			_bg.element = _normalElement;	
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_bg.element = _normalElement;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.localRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this, EventArgs.Empty);
		}
	}
	
	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_bg.element = _normalElement;
	}
	
	
}


