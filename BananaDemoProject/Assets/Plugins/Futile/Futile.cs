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
	
	public event EventHandler SignalUpdate;
	public event EventHandler SignalLateUpdate;
	
	public int drawDepth = 100;
	
	private GameObject _cameraHolder;
	private Camera _camera;
	
	public int targetFrameRate = 60;
	
	//this is populated by the FutileParams
	private float _originX;
	private float _originY;
	
	public static int startingQuadsPerLayer;
	public static int quadsPerLayerExpansion;
	
	private FFrameworkParams _engineParams;
	private FResolutionLevel _resLevel;

	// Use this for initialization
	private void Awake () 
	{
		instance = this;
		isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
	}
	
	public void Init(FFrameworkParams engineParams)
	{
		Application.targetFrameRate = targetFrameRate;
		
		_engineParams = engineParams;
		
		Futile.startingQuadsPerLayer = _engineParams.startingQuadsPerLayer;
		Futile.quadsPerLayerExpansion = _engineParams.quadsPerLayerExpansion;
		
		float length = Math.Max(Screen.height, Screen.width);
		
		
		//get the resolution level - the one we're closest to WITHOUT going over, price is right rules :)
		_resLevel = null;
		
		foreach(FResolutionLevel resLevel in _engineParams.resLevels)
		{
			if(length <= resLevel.maxLength) //we've found our resLevel
			{
				_resLevel = resLevel;
				break;
			}
		}
		
		//if we couldn't find a res level, it means the screen is bigger than the biggest one, so just choose that one
		if(_resLevel == null)
		{
			_resLevel = _engineParams.resLevels.GetLastObject();	
			if(_resLevel == null)
			{
				throw new Exception("You must add at least one FResolutionLevel!");	
			}
		}
		
		Futile.resourceSuffix = _resLevel.resourceSuffix;
		
		//this is what helps us figure out the display scale if we're not at a specific resolution level
		//it's relative to the next highest resolution level
		float displayScaleModifier = length/_resLevel.maxLength;
			
		
		displayScale = _resLevel.displayScale * displayScaleModifier;
		displayScaleInverse = 1.0f/displayScale;
		
		resourceScale = _resLevel.resourceScale;
		resourceScaleInverse = 1.0f/resourceScale;
		
		contentScale = _resLevel.contentScale;
		contentScaleInverse = 1.0f/contentScale;

		width = Screen.width*displayScaleInverse;
		height = Screen.height*displayScaleInverse;
		
		halfWidth = width/2.0f;
		halfHeight = height/2.0f;
		
		_originX = _engineParams.origin.x;
		_originY = _engineParams.origin.y;
		
		Debug.Log ("Futile: Display scale is " + displayScale);
		
		Debug.Log ("Futile: Content scale is " + contentScale);
		
		Debug.Log ("Futile: Resource scale is " + resourceScale);
		
		Debug.Log ("Futile: Resource suffix is " + _resLevel.resourceSuffix);
		
		Debug.Log ("Futile: Screen size in pixels is (" + Screen.width +"px," + Screen.height+"px)");
		
		Debug.Log ("Futile: Screen size in points is (" + width + "," + height+")");
		
		Debug.Log ("Futile: Origin is at (" + _originX*width + "," + _originY*height+")");
		
		//
		//Camera setup from https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UI.cs
		//
				
		name = "Futile"; 
		
		_cameraHolder = new GameObject();
		_cameraHolder.transform.parent = gameObject.transform;
		_cameraHolder.AddComponent<Camera>();
		
		_camera = _cameraHolder.camera;
		_camera.name = "FCamera";
		//_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera.nearClipPlane = -50.3f;
		_camera.farClipPlane = 50.0f;
		_camera.depth = drawDepth;
		_camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		_camera.backgroundColor = Color.black;
		
		//we multiply this stuff by scaleInverse to make sure everything is in points, not pixels
		_camera.orthographic = true;
		_camera.orthographicSize = Screen.height/2 * displayScaleInverse;

		UpdateCameraPosition();
		
		touchManager = new FTouchManager();
		
		atlasManager = new FAtlasManager();
		
		stage = new FStage();
	}
	
	protected void Update()
	{
		touchManager.Update();
		if(SignalUpdate != null) SignalUpdate(this, EventArgs.Empty);
		stage.Redraw (false,false);
	}
	
	protected void LateUpdate()
	{
		stage.LateUpdate();
		if(SignalLateUpdate != null) SignalLateUpdate(this, EventArgs.Empty);
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
		float camXOffset = ((_originX - 0.5f) * -Screen.width)*displayScaleInverse;
		float camYOffset = ((_originY - 0.5f) * -Screen.height)*displayScaleInverse;
	
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

	

}
