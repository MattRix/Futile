using UnityEngine;
using System.Collections;
using System;

public class FButton : FContainer, FSingleTouchableInterface
{
	protected FAtlasElement _upElement;
	protected FAtlasElement _downElement;
	protected FSprite _bg;
	protected string _soundName;
	protected FLabel _label;

	public event Action<FButton> SignalPress;
	public event Action<FButton> SignalRelease;
	public event Action<FButton> SignalReleaseOutside;

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;
	
	public float expansionAmount = 10;

	public FButton (string upElementName, string downElementName, string soundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		_bg = new FSprite(_upElement.name);
		_bg.anchorX = _anchorX;
		_bg.anchorY = _anchorY;
		AddChild(_bg);

		_soundName = soundName;
	}
	// Simpler constructors
	public FButton (string upImage, string downImage) : this(upImage, downImage, null) {}
	public FButton (string upImage) : this(upImage, upImage, null) {}

	public FSprite sprite
	{
		get { return _bg;}
	}

	public float anchorX
	{
		set
		{
			_anchorX = value;
			_bg.anchorX = _anchorX;
			if (_label != null) _label.x = -_anchorX*_bg.width+_bg.width/2;
		}
		get {return _anchorX;}
	}

	public float anchorY
	{
		set
		{
			_anchorY = value;
			_bg.anchorY = _anchorY;
			if (_label != null) _label.y = -_anchorY*_bg.height+_bg.height/2;
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
		
		if(_bg.textureRect.Contains(touchPos))
		{
			_bg.element = _downElement;
			
			if(_soundName != null) FSoundManager.PlaySound(_soundName);
			
			if(SignalPress != null) SignalPress(this);
			
			return true;	
		}
		
		return false;
	}
	
	public void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _bg.textureRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			_bg.element = _downElement;	
		}
		else
		{
			_bg.element = _upElement;	
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_bg.element = _upElement;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _bg.textureRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this);
		}
		else
		{
			if(SignalReleaseOutside != null) SignalReleaseOutside(this);	
		}
	}
	
	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_bg.element = _upElement;
		if(SignalReleaseOutside != null) SignalReleaseOutside(this);
	}
	
	
}


