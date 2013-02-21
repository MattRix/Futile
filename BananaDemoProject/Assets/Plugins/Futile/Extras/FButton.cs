using UnityEngine;
using System.Collections;
using System;

public class FButton : FContainer, FSingleTouchableInterface
{
	protected Rect _hitRect;
	protected bool _shouldUseCustomHitRect = false;
	
	protected FAtlasElement _upElement;
	protected FAtlasElement _downElement;
	protected FAtlasElement _overElement;
	
	protected FSprite _sprite;
	protected string _clickSoundName;
	protected FLabel _label;
	
	public delegate void ButtonSignalDelegate(FButton button);
	
	public event ButtonSignalDelegate SignalPress;
	public event ButtonSignalDelegate SignalRelease;
	public event ButtonSignalDelegate SignalReleaseOutside;

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;
	
	public float expansionAmount = 10;
	
	protected bool _isEnabled = true;
	protected bool _supportsOver = false;
	protected bool _isTouchDown = false;
	
	public FButton (string upElementName, string downElementName, string overElementName, string clickSoundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		
		if(overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		
		_sprite = new FSprite(_upElement.name);
		_sprite.anchorX = _anchorX;
		_sprite.anchorY = _anchorY;
		AddChild(_sprite);
		
		_hitRect = _sprite.textureRect;

		_clickSoundName = clickSoundName;
	}
	
	// Simpler constructors
	public FButton (string upElementName) : 
		this(upElementName, upElementName, null, null) {}
	
	public FButton (string upElementName, string downElementName) : 
		this(upElementName, downElementName, null, null) {}
	
	public FButton (string upElementName, string downElementName, string clickSoundName) : 
		this(upElementName, downElementName, null, clickSoundName) {}
	
	public void SetElements(string upElementName, string downElementName, string overElementName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		
		if(overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		
		if(_isTouchDown)
		{
			_sprite.element = _downElement;	
		}
		else 
		{
			_sprite.element = _upElement;
		}
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
		_label.x = -_anchorX*_sprite.width+_sprite.width/2;
		_label.y = -_anchorY*_sprite.height+_sprite.height/2;
		
		return _label;
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
		
		if(_hitRect.Contains(mousePos))
		{
			_sprite.element = _overElement;
		}
		else 
		{
			_sprite.element = _upElement;
		}
	}
	
	public bool HandleSingleTouchBegan(FTouch touch)
	{
		_isTouchDown = false;
		
		if(!_isVisible) return false;
		
		if(!_shouldUseCustomHitRect)
		{
			_hitRect = _sprite.textureRect;
		}
		
		Vector2 touchPos = _sprite.GlobalToLocal(touch.position);
		
		if(_hitRect.Contains(touchPos))
		{
			if(_isEnabled) //swallow touches all the time, but only listen to them when enabled
			{
				_sprite.element = _downElement;
				
				if(_clickSoundName != null) FSoundManager.PlaySound(_clickSoundName);
				
				if(SignalPress != null) SignalPress(this);
				
				_isTouchDown = true;
			}
			
			return true;	
		}
		
		return false;
	}
	
	public void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 touchPos = _sprite.GlobalToLocal(touch.position);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _hitRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			_sprite.element = _downElement;	
			_isTouchDown = true;
		}
		else
		{
			_sprite.element = _upElement;	
			_isTouchDown = false;
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_isTouchDown = false;
		
		_sprite.element = _upElement;
		
		Vector2 touchPos = _sprite.GlobalToLocal(touch.position);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _hitRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this);
			
			if(_supportsOver && _hitRect.Contains(touchPos)) //go back to the over image if we're over the button
			{
				_sprite.element = _overElement;	
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
		
		_sprite.element = _upElement;
		if(SignalReleaseOutside != null) SignalReleaseOutside(this);
	}
	
	public FLabel label
	{
		get {return _label;}
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
	
	public FSprite sprite
	{
		get { return _sprite;}
	}

	public float anchorX
	{
		set
		{
			_anchorX = value;
			_sprite.anchorX = _anchorX;
			if (_label != null) _label.x = -_anchorX*_sprite.width+_sprite.width/2;
		}
		get {return _anchorX;}
	}

	public float anchorY
	{
		set
		{
			_anchorY = value;
			_sprite.anchorY = _anchorY;
			if (_label != null) _label.y = -_anchorY*_sprite.height+_sprite.height/2;
		}
		get {return _anchorY;}
	}
	
	
	//you can set a custom hitRect to be used instead of the upElement's rect
	//but it's important to remember that the hitRect is in local coordinates
	public Rect hitRect
	{
		get {return _hitRect;}
		set 
		{
			_hitRect = value; 
			_shouldUseCustomHitRect = true;
		}
	}
	
	//for convenience
	public void SetAnchor(float newX, float newY)
	{
		this.anchorX = newX;
		this.anchorY = newY;
	}
	
	public void SetAnchor(Vector2 newAnchor)
	{
		this.anchorX = newAnchor.x;
		this.anchorY = newAnchor.y;
	}
	
	public Vector2 GetAnchor()
	{
		return new Vector2(_anchorX,_anchorY);	
	}
	
	
}


