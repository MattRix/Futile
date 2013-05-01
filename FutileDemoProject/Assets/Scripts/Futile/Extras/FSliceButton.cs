using UnityEngine;
using System.Collections;
using System;

public class FSliceButton : FContainer, FSingleTouchableInterface
{
	protected FAtlasElement _upElement;
	protected FAtlasElement _downElement;
	protected Color _upColor;
	protected Color _downColor;
	protected FSliceSprite _bg;
	protected string _soundName;
	protected FLabel _labelA;
	protected FLabel _labelB;
	
	public delegate void FSliceButtonSignalDelegate(FSliceButton button);

	public event FSliceButtonSignalDelegate SignalPress;
	public event FSliceButtonSignalDelegate SignalRelease;
	public event FSliceButtonSignalDelegate SignalReleaseOutside;

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;
	
	public float expansionAmount = 10;
	
	private bool _isEnabled = true;
	
	public FSliceButton (float width, float height, string upElementName, string downElementName, Color upColor, Color downColor, string soundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		_upColor = upColor;
		_downColor = downColor;
		
		_soundName = soundName;
					
		_bg = new FSliceSprite(_upElement.name, width, height, 16, 16, 16, 16);
		_bg.anchorX = _anchorX;
		_bg.anchorY = _anchorY;
		_bg.color = _upColor;
		AddChild(_bg);
	}
	
	// Simpler constructors
	public FSliceButton(float width, float height, string upElementName, string downElementName, Color color, string soundName)
		: this(width, height, upElementName, downElementName, color, color, soundName) {}

	public FSliceButton(float width, float height, string upElementName, string downElementName, Color color)
		: this(width, height, upElementName, downElementName, color, color, null) {}

	public FSliceButton(float width, float height, string upElementName, string downElementName, string soundName)
		: this(width, height, upElementName, downElementName, Color.white, Color.white, soundName) {}

	public FSliceButton(float width, float height, string upElementName, string downElementName)
		: this(width, height, upElementName, downElementName, Color.white, Color.white, null) {}
	
	public FLabel AddLabelA (string fontName, string text, float scale, float offsetY, Color color)
	{
		if(_labelA != null) 
		{
			RemoveChild(_labelA);
		}

		_labelA = new FLabel(fontName, text);
		AddChild(_labelA);
		_labelA.color = color;
		_labelA.anchorX = _labelA.anchorY = 0.5f;
		_labelA.x = -_anchorX*_bg.width+_bg.width/2;
		_labelA.y = -_anchorY*_bg.height+_bg.height/2 + offsetY;
		_labelA.scale = scale;
		
		return _labelA;
	}
	
	public FLabel AddLabelB (string fontName, string text, float scale, float offsetY, Color color)
	{
		if(_labelB != null) 
		{
			RemoveChild(_labelB);
		}

		_labelB = new FLabel(fontName, text);
		AddChild(_labelB);
		_labelB.color = color;
		_labelB.anchorX = _labelB.anchorY = 0.5f;
		_labelB.x = -_anchorX*_bg.width+_bg.width/2;
		_labelB.y = -_anchorY*_bg.height+_bg.height/2 + offsetY;
		_labelB.scale = scale;
		
		return _labelB;
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
		if(!_isEnabled) return false;
		
		Vector2 touchPos = _bg.GlobalToLocal(touch.position);
		
		if(_bg.textureRect.Contains(touchPos))
		{
			_bg.element = _downElement;
			_bg.color = _downColor;
			
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
			_bg.color = _downColor;
		}
		else
		{
			_bg.element = _upElement;	
			_bg.color = _upColor;
		}
	}
	
	public void HandleSingleTouchEnded(FTouch touch)
	{
		_bg.element = _upElement;
		_bg.color = _upColor;
		
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
		_bg.color = _upColor;
		if(SignalReleaseOutside != null) SignalReleaseOutside(this);
	}
	
	public void SetAnchor(float x, float y)
	{
		this.anchorX = x;
		this.anchorY = y;
	}
	
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
			if (_labelA != null) _labelA.x = -_anchorX*_bg.width+_bg.width/2;
			if (_labelB != null) _labelB.x = -_anchorX*_bg.width+_bg.width/2;
		}
		get {return _anchorX;}
	}

	public float anchorY
	{
		set
		{
			_anchorY = value;
			_bg.anchorY = _anchorY;
			if (_labelA != null) _labelA.y = -_anchorY*_bg.height+_bg.height/2;
			if (_labelB != null) _labelB.y = -_anchorY*_bg.height+_bg.height/2;
		}
		get {return _anchorY;}
	}

	public bool isEnabled
	{
		get {return _isEnabled;}
		set 
		{
			if(_isEnabled != value)
			{
				_isEnabled = value;
				
				if(_isEnabled)
				{
					_bg.color = _upColor;
				}
				else
				{
					RXColorHSL hsl = RXColor.HSLFromColor(_upColor);
					hsl.s = 0.25f;
					hsl.l = 0.6f;
					Color greyscale = RXColor.ColorFromHSL(hsl);
					_bg.color = greyscale;
				}
			}
		}
	}
	
	public FLabel labelA
	{
		get {return _labelA;}
	}
	
	public FLabel labelB
	{
		get {return _labelB;}
	}
	
	public float width
	{
		get { return _bg.width; }
		set { _bg.width = value; } 
	}
	
	public float height
	{
		get { return _bg.height; }
		set { _bg.height = value; } 
	}	
}


