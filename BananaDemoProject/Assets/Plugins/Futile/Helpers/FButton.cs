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

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;

	public FButton (string upImage, string downImage, string sound)
	{
		_normalElement = FEngine.atlasManager.GetElementWithName(upImage);
		_pressedElement = FEngine.atlasManager.GetElementWithName(downImage);
		_bg = new FSprite(_normalElement.name);
		_bg.anchorX = _anchorX;
		_bg.anchorY = _anchorY;
		AddChild(_bg);

		_sound = sound;
	}
	// Simpler constructors
	public FButton (string upImage, string downImage) : this(upImage, downImage, null) {}
	public FButton (string upImage) : this(upImage, upImage, null) {}

	public float anchorX
	{
		set
		{
			_anchorX = value;
			_bg.anchorX = _anchorX;
			if (_label != null)
				_label.x = -_anchorX*_bg.width+_bg.width/2;
		}
		get {return _anchorX;}
	}

	public float anchorY
	{
		set
		{
			_anchorY = value;
			_bg.anchorY = _anchorY;
			if (_label != null)
				_label.y = -_anchorY*_bg.height+_bg.height/2;
		}
		get {return _anchorY;}
	}

	public void AddLabel (string fontName, string text, Color color)
	{
		if(_label != null) 
		{
			RemoveChild(_label);
		}

		_label = new FLabel(fontName, text);
		AddChild(_label);
		_label.color = color;
		_label.anchorX = _label.anchorY = 0.5f;
		_label.x = -_anchorX*_bg.width+_bg.width/2;
		_label.y = -_anchorY*_bg.height+_bg.height/2;
		//Debug.Log("Label height:"+_label.
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
		
		if(_bg.boundsRect.Contains(touchPos))
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
		
		if(_bg.boundsRect.Contains(touchPos))
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
		
		if(_bg.boundsRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this, EventArgs.Empty);
		}
	}
	
	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_bg.element = _normalElement;
	}
	
	
}


