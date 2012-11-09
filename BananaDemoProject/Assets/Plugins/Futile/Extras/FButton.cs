using UnityEngine;
using System.Collections;
using System;

public class FButton : FContainer, FSingleTouchableInterface
{
	protected FAtlasElement _upElement;
	protected FAtlasElement _downElement;
	protected FAtlasElement _overElement;
	
	protected FSprite _bg;
	protected string _clickSoundName;
	protected FLabel _label;

	public event Action<FButton> SignalPress;
	public event Action<FButton> SignalRelease;
	public event Action<FButton> SignalReleaseOutside;

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;
	
	public float expansionAmount = 10;
	
	private bool _isEnabled = true;
	
	private bool _supportsOver = false;
	
	private bool _isTouchDown = false;
	
	public FButton (string upElementName, string downElementName, string overElementName, string clickSoundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		
		if(overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		
		_bg = new FSprite(_upElement.name);
		_bg.anchorX = _anchorX;
		_bg.anchorY = _anchorY;
		AddChild(_bg);

		_clickSoundName = clickSoundName;
	}
	
	// Simpler constructors
	public FButton (string upImage) : 
		this(upImage, upImage, null, null) {}
	
	public FButton (string upImage, string downImage) : 
		this(upImage, downImage, null, null) {}
	
	public FButton (string upImage, string downImage, string clickSoundName) : 
		this(upImage, downImage, null, clickSoundName) {}

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

	public FLabel AddLabel (string fontName, string text, Color color)
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
		
		return _label;
	}

	public FLabel label
	{
		get {return _label;}
	}

	override public void HandleAddedToStage()
	{
		base.HandleAddedToStage();	
		Futile.touchManager.AddSingleTouchTarget(this);
		
		if(_supportsOver)
		{
			Futile.instance.SignalUpdate += HandleUpdate;
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		base.HandleRemovedFromStage();	
		Futile.touchManager.RemoveSingleTouchTarget(this);
		
		if(_supportsOver)
		{
			Futile.instance.SignalUpdate -= HandleUpdate;
		}
	}
	
	private void HandleUpdate()
	{
		UpdateOverState();
	}
	
	private void UpdateOverState()
	{
		if(_isTouchDown) return; //if the touch is down then we don't have to worry about over states
		
		Vector2 mousePos = GetLocalMousePosition();
		
		if(_bg.textureRect.Contains(mousePos))
		{
			_bg.element = _overElement;
		}
		else 
		{
			_bg.element = _upElement;
		}
	}
	
	public bool HandleSingleTouchBegan(FTouch touch)
	{
		_isTouchDown = false;
		
		if(!_isEnabled) return false;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.textureRect.Contains(touchPos))
		{
			_bg.element = _downElement;
			
			if(_clickSoundName != null) FSoundManager.PlaySound(_clickSoundName);
			
			if(SignalPress != null) SignalPress(this);
			
			_isTouchDown = true;
			
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
			_isTouchDown = true;
		}
		else
		{
			_bg.element = _upElement;	
			_isTouchDown = false;
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_isTouchDown = false;
		
		_bg.element = _upElement;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _bg.textureRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this);
			
			if(_supportsOver && _bg.textureRect.Contains(touchPos)) //go back to the over image if we're over the button
			{
				_bg.element = _overElement;	
			}
		}
		else
		{
			if(SignalReleaseOutside != null) SignalReleaseOutside(this);	
		}
	}
	
	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_isTouchDown = false;
		
		_bg.element = _upElement;
		if(SignalReleaseOutside != null) SignalReleaseOutside(this);
	}
	
	public bool isEnabled
	{
		get {return _isEnabled;}
		set 
		{
			if(_isEnabled != value)
			{
				_isEnabled = value;
			}
		}
	}
	
	
}


