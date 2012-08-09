using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FacileEngine by Matt Rix - 

public class Futile : MonoBehaviour 
{
	static public Futile instance = null;
	
	static public FAtlasManager atlasManager;
	
	static public FStage stage;
	
	static public FTouchManager touchManager;
	
	static public bool isOpenGL; //assigned in Awake
	
	static public float displayScale; //set based on the resolution setting (the unit to pixel scale)
	static public float displayScaleInverse; // 1/displayScale
	
	static public float contentScale; //should usually be 1.0f except in rare circumstances
	static public float contentScaleInverse; 
	
	static public float resourceScale; //set based on the resolution setting (the scale of assets)
	static public float resourceScaleInverse; // 1/resourceScale
	
	static public float width; //in points, not pixels
	static public float height; //in points, not pixels
	
	static public float halfWidth; //in points
	static public float halfHeight; //in points
	
	static public string resourceSuffix; //set based on the resLevel
	
	static public float screenWidth;
	static public float screenHeight;
	
	public event Action SignalUpdate;
	public event Action SignalLateUpdate;
	
	public event Action SignalOrientationChange;
	public event Action<bool> SignalResize; //the bool represents wasOrientationChange
	
	public int drawDepth = 100;
	
	private GameObject _cameraHolder;
	private Camera _camera;
	
	//this is populated by the FutileParams
	private float _originX;
	private float _originY;
	
	public static int startingQuadsPerLayer;
	public static int quadsPerLayerExpansion;
	
	private FutileParams _futileParams;
	private FResolutionLevel _resLevel;
	
	private ScreenOrientation _currentOrientation;
	
	private float _screenLongLength;
	private float _screenShortLength;
	
	// Use this for initialization
	private void Awake () 
	{
		instance = this;
		isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
	}
	
	public void Init(FutileParams futileParams)
	{	
		_futileParams = futileParams;
		
		Application.targetFrameRate = _futileParams.targetFrameRate;
		
		
		TouchScreenKeyboard.autorotateToLandscapeLeft = false;
		TouchScreenKeyboard.autorotateToLandscapeRight = false;
		TouchScreenKeyboard.autorotateToPortrait = false;
		TouchScreenKeyboard.autorotateToPortraitUpsideDown = false;
		
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

		Futile.startingQuadsPerLayer = _futileParams.startingQuadsPerLayer;
		Futile.quadsPerLayerExpansion = _futileParams.quadsPerLayerExpansion;
		
		_screenLongLength = Math.Max(Screen.height, Screen.width);
		_screenShortLength = Math.Min(Screen.height, Screen.width);
		
		if(_currentOrientation == ScreenOrientation.Portrait || _currentOrientation == ScreenOrientation.PortraitUpsideDown)
		{
			screenWidth = _screenShortLength;
			screenHeight = _screenLongLength;
		}
		else //landscape
		{
			screenWidth = _screenLongLength;
			screenHeight = _screenShortLength;
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
		
		//if we couldn't find a res level, it means the screen is bigger than the biggest one, so just choose that one
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
		float displayScaleModifier = _screenLongLength/_resLevel.maxLength;
			
		
		displayScale = _resLevel.displayScale * displayScaleModifier;
		displayScaleInverse = 1.0f/displayScale;
		
		resourceScale = _resLevel.resourceScale;
		resourceScaleInverse = 1.0f/resourceScale;
		
		contentScale = _resLevel.contentScale;
		contentScaleInverse = 1.0f/contentScale;

		width = screenWidth*displayScaleInverse;
		height = screenHeight*displayScaleInverse;
		
		halfWidth = width/2.0f;
		halfHeight = height/2.0f;
		
		_originX = _futileParams.origin.x;
		_originY = _futileParams.origin.y;
		
		Debug.Log ("Futile: Display scale is " + displayScale);
		
		Debug.Log ("Futile: Content scale is " + contentScale);
		
		Debug.Log ("Futile: Resource scale is " + resourceScale);
		
		Debug.Log ("Futile: Resource suffix is " + _resLevel.resourceSuffix);
		
		Debug.Log ("Futile: Screen size in pixels is (" + screenWidth +"px," + screenHeight+"px)");
		
		Debug.Log ("Futile: Screen size in points is (" + width + "," + height+")");
		
		Debug.Log ("Futile: Origin is at (" + _originX*width + "," + _originY*height+")");
		
		Debug.Log ("Futile: Initial orientation is " + _currentOrientation);
		
		//
		//Camera setup from https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UI.cs
		//
				
		name = "Futile"; 
		
		_cameraHolder = new GameObject();
		_cameraHolder.transform.parent = gameObject.transform;
		_cameraHolder.AddComponent<Camera>();
		
		_camera = _cameraHolder.camera;
		_camera.name = "FCamera";
		_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera.nearClipPlane = -50.3f;
		_camera.farClipPlane = 50.0f;
		_camera.depth = drawDepth;
		_camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		_camera.backgroundColor = Color.black;
		
		//we multiply this stuff by scaleInverse to make sure everything is in points, not pixels
		_camera.orthographic = true;
		_camera.orthographicSize = screenHeight/2 * displayScaleInverse;

		UpdateCameraPosition();
		
		touchManager = new FTouchManager();
		
		atlasManager = new FAtlasManager();
		
		stage = new FStage();
	}

	protected void SwitchOrientation (ScreenOrientation newOrientation)
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
			
			if(_currentOrientation == ScreenOrientation.Portrait || _currentOrientation == ScreenOrientation.PortraitUpsideDown)
			{
				screenWidth = _screenShortLength;
				screenHeight = _screenLongLength;
			}
			else //landscape
			{
				screenWidth = _screenLongLength;
				screenHeight = _screenShortLength;
			}
			
			width = screenWidth*displayScaleInverse;
			height = screenHeight*displayScaleInverse;
			
			halfWidth = width/2.0f;
			halfHeight = height/2.0f;
			
			_camera.orthographicSize = screenHeight/2 * displayScaleInverse;
			UpdateCameraPosition(); 
			
			Debug.Log ("Orientating switched to " + _currentOrientation + " screen is now: " + screenWidth+","+screenHeight);
			
			if(SignalOrientationChange != null) SignalOrientationChange();
			if(SignalResize != null) SignalResize(true);
		}
	}
	
	protected void Update()
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

		touchManager.Update();
		if(SignalUpdate != null) SignalUpdate();
		stage.Redraw (false,false);
	}
	
	protected void LateUpdate()
	{
		stage.LateUpdate();
		if(SignalLateUpdate != null) SignalLateUpdate();
	}	
	
	protected void OnApplicationQuit()
	{
		instance = null;
	}
	
	protected void OnDestroy()
	{
		instance = null;	
	}
	
	protected void UpdateCameraPosition()
	{
		float camXOffset = ((_originX - 0.5f) * -screenWidth)*displayScaleInverse;
		float camYOffset = ((_originY - 0.5f) * -screenHeight)*displayScaleInverse;
	
		_camera.transform.position = new Vector3(camXOffset, camYOffset, -10.0f); 	
	}

	public float originX
	{
		get {return _originX;}
		set 
		{
			if(_originX != value)
			{
				_originX = value;
				UpdateCameraPosition();
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
				UpdateCameraPosition();
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

