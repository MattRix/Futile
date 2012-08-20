using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FScreen
{
	public float width; //in points, not pixels
	public float height; //in points, not pixels
	
	public float halfWidth; //for convenience, in points
	public float halfHeight; //for convenience, in points

	public float pixelWidth; //in pixels
	public float pixelHeight; //in pixels
	
	
	
	public event Action SignalOrientationChange;
	
	//the bool in SignalResize represents wasOrientationChange,
	//which tells you whether the resize was due to an orientation change or not
	public event Action<bool> SignalResize; 
	
	
	
	
	
	//this is populated by the FutileParams
	private float _originX;
	private float _originY;
	
	private ScreenOrientation _currentOrientation;
	
	private float _screenLongLength;
	private float _screenShortLength;
	
	private bool _didJustResize;
	private float _oldWidth;
	private float _oldHeight;
	
	private FResolutionLevel _resLevel;
	
	private FutileParams _futileParams;
	
	public FScreen (FutileParams futileParams)
	{
		_futileParams = futileParams;
		#if UNITY_IPHONE || UNITY_ANDROID
		TouchScreenKeyboard.autorotateToLandscapeLeft = false;
		TouchScreenKeyboard.autorotateToLandscapeRight = false;
		TouchScreenKeyboard.autorotateToPortrait = false;
		TouchScreenKeyboard.autorotateToPortraitUpsideDown = false;
		#endif
		
		//Non-mobile unity always defaults to portrait for some reason, so fix this manually
		if(Screen.height > Screen.width)
		{
			_currentOrientation = ScreenOrientation.Portrait;	
		}
		else
		{
			_currentOrientation = ScreenOrientation.LandscapeLeft;
		}
		
		//get the correct orientation if we're on a mobile platform
		#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
			_currentOrientation = Screen.orientation;
		#endif
				
		//special "single orientation" mode
		if(_futileParams.singleOrientation != ScreenOrientation.Unknown)
		{
			_currentOrientation = _futileParams.singleOrientation;
		}
		else //if we're not in a supported orientation, put us in one!
		{
			if(_currentOrientation == ScreenOrientation.LandscapeLeft && !_futileParams.supportsLandscapeLeft)
			{
				if(_futileParams.supportsLandscapeRight) _currentOrientation = ScreenOrientation.LandscapeRight;
				else if(_futileParams.supportsPortrait) _currentOrientation = ScreenOrientation.Portrait;
				else if(_futileParams.supportsPortraitUpsideDown) _currentOrientation = ScreenOrientation.PortraitUpsideDown;
			}	
			else if(_currentOrientation == ScreenOrientation.LandscapeRight && !_futileParams.supportsLandscapeRight)
			{
				if(_futileParams.supportsLandscapeLeft) _currentOrientation = ScreenOrientation.LandscapeLeft;
				else if(_futileParams.supportsPortrait) _currentOrientation = ScreenOrientation.Portrait;
				else if(_futileParams.supportsPortraitUpsideDown) _currentOrientation = ScreenOrientation.PortraitUpsideDown;
			}
			else if(_currentOrientation == ScreenOrientation.Portrait && !_futileParams.supportsPortrait)
			{
				if(_futileParams.supportsPortraitUpsideDown) _currentOrientation = ScreenOrientation.PortraitUpsideDown;
				else if(_futileParams.supportsLandscapeLeft) _currentOrientation = ScreenOrientation.LandscapeLeft;
				else if(_futileParams.supportsLandscapeRight) _currentOrientation = ScreenOrientation.LandscapeRight;
			}
			else if(_currentOrientation == ScreenOrientation.PortraitUpsideDown && !_futileParams.supportsPortraitUpsideDown)
			{
				if(_futileParams.supportsPortrait) _currentOrientation = ScreenOrientation.Portrait;
				else if(_futileParams.supportsLandscapeLeft) _currentOrientation = ScreenOrientation.LandscapeLeft;
				else if(_futileParams.supportsLandscapeRight) _currentOrientation = ScreenOrientation.LandscapeRight;
			}
		}
		
		Screen.orientation = _currentOrientation;

		_screenLongLength = Math.Max(Screen.height, Screen.width);
		_screenShortLength = Math.Min(Screen.height, Screen.width);
		
		if(_currentOrientation == ScreenOrientation.Portrait || _currentOrientation == ScreenOrientation.PortraitUpsideDown)
		{
			pixelWidth = _screenShortLength;
			pixelHeight = _screenLongLength;
		}
		else //landscape
		{
			pixelWidth = _screenLongLength;
			pixelHeight = _screenShortLength;
		}
		
		
		//get the resolution level - the one we're closest to WITHOUT going over, price is right rules :)
		_resLevel = null;
		
		foreach(FResolutionLevel resLevel in _futileParams.resLevels)
		{
			if(_screenLongLength <= resLevel.maxLength) //we've found our resLevel
			{
				_resLevel = resLevel;
				break;
			}
		}
		
		//if we couldn't find a res level, it means the screen is bigger than the biggest one, so just choose the biggest
		if(_resLevel == null)
		{
			_resLevel = _futileParams.resLevels.GetLastObject();	
			if(_resLevel == null)
			{
				throw new Exception("You must add at least one FResolutionLevel!");	
			}
		}
		
		Futile.resourceSuffix = _resLevel.resourceSuffix;
		
		//this is what helps us figure out the display scale if we're not at a specific resolution level
		//it's relative to the next highest resolution level
		
		float displayScaleModifier = 1.0f;
		
		if(_futileParams.shouldLerpToNearestResolutionLevel)
		{
			displayScaleModifier = _screenLongLength/_resLevel.maxLength;
		}
		 
		Futile.displayScale = _resLevel.displayScale * displayScaleModifier;
		Futile.displayScaleInverse = 1.0f/Futile.displayScale;
		
		Futile.resourceScale = _resLevel.resourceScale;
		Futile.resourceScaleInverse = 1.0f/Futile.resourceScale;

		width = pixelWidth*Futile.displayScaleInverse;
		height = pixelHeight*Futile.displayScaleInverse;
		
		halfWidth = width/2.0f;
		halfHeight = height/2.0f;
		
		_originX = _futileParams.origin.x;
		_originY = _futileParams.origin.y;
		
		Debug.Log ("Futile: Display scale is " + Futile.displayScale);
		
		Debug.Log ("Futile: Resource scale is " + Futile.resourceScale);
		
		Debug.Log ("Futile: Resource suffix is " + _resLevel.resourceSuffix);
		
		Debug.Log ("FScreen: Screen size in pixels is (" + pixelWidth +"px," + pixelHeight+"px)");
		
		Debug.Log ("FScreen: Screen size in points is (" + width + "," + height+")");
		
		Debug.Log ("FScreen: Origin is at (" + _originX*width + "," + _originY*height+")");
		
		Debug.Log ("FScreen: Initial orientation is " + _currentOrientation);
		
		_didJustResize = true;
	}
	
	public void Update()
	{
		if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft && _currentOrientation != ScreenOrientation.LandscapeLeft && _futileParams.supportsLandscapeLeft)
		{
			SwitchOrientation(ScreenOrientation.LandscapeLeft);
		}
		else if(Input.deviceOrientation == DeviceOrientation.LandscapeRight && _currentOrientation != ScreenOrientation.LandscapeRight && _futileParams.supportsLandscapeRight)
		{
			SwitchOrientation(ScreenOrientation.LandscapeRight);
		}
		else if(Input.deviceOrientation == DeviceOrientation.Portrait && _currentOrientation != ScreenOrientation.Portrait && _futileParams.supportsPortrait)
		{
			SwitchOrientation(ScreenOrientation.Portrait);
		}
		else if(Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown && _currentOrientation != ScreenOrientation.PortraitUpsideDown && _futileParams.supportsPortraitUpsideDown)
		{
			SwitchOrientation(ScreenOrientation.PortraitUpsideDown);
		}
		
		if(_didJustResize)
		{
			_didJustResize = false;
			_oldWidth = Screen.width;
			_oldHeight = Screen.height;
		}
		else if(_oldWidth != Screen.width || _oldHeight != Screen.height)
		{
			_oldWidth = Screen.width;
			_oldHeight = Screen.height;
			
			UpdateScreenDimensions();
			if(SignalResize != null) SignalResize(false);
		}	
	}
	
	public void SwitchOrientation (ScreenOrientation newOrientation)
	{
		Debug.Log("Futile: Orientation changed to " + newOrientation);
				
		if(_futileParams.singleOrientation != ScreenOrientation.Unknown) //if we're in single orientation mode, just broadcast the change, don't actually change anything
		{
			_currentOrientation = newOrientation;
			if(SignalOrientationChange != null) SignalOrientationChange();
		}
		else
		{
			Screen.orientation = newOrientation;
			_currentOrientation = newOrientation;
			
			UpdateScreenDimensions();
			
			Debug.Log ("Orientation switched to " + _currentOrientation + " screen is now: " + pixelWidth+"x"+pixelHeight+"px");
			
			if(SignalOrientationChange != null) SignalOrientationChange();
			if(SignalResize != null) SignalResize(true);
			
			_didJustResize = true;
		}
	}
	
	private void UpdateScreenDimensions()
	{
		_screenLongLength = Math.Max (Screen.width, Screen.height);
		_screenShortLength = Math.Min (Screen.width, Screen.height);
		
		if(_currentOrientation == ScreenOrientation.Portrait || _currentOrientation == ScreenOrientation.PortraitUpsideDown)
		{
			pixelWidth = _screenShortLength;
			pixelHeight = _screenLongLength;
		}
		else //landscape
		{
			pixelWidth = _screenLongLength;
			pixelHeight = _screenShortLength;
		}
		
		width = pixelWidth*Futile.displayScaleInverse;
		height = pixelHeight*Futile.displayScaleInverse;
		
		halfWidth = width/2.0f;
		halfHeight = height/2.0f;
		
		Futile.instance.UpdateCameraPosition(); 	
	}
	
	public float originX
	{
		get {return _originX;}
		set 
		{
			if(_originX != value)
			{
				_originX = value;
				Futile.instance.UpdateCameraPosition();
			}
		}
	}

	public float originY
	{
		get {return _originY;}
		set 
		{
			if(_originY != value)
			{
				_originY = value;
				Futile.instance.UpdateCameraPosition();
			}
		}
	}
	
	public ScreenOrientation currentOrientation
	{
		get {return _currentOrientation;}
		set 
		{
			if(_currentOrientation != value)
			{
				SwitchOrientation(value);
			}	
		}
	}
	
	public bool IsLandscape()
	{
		return _currentOrientation == ScreenOrientation.LandscapeLeft || _currentOrientation == ScreenOrientation.LandscapeRight;
	}
}


