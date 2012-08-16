using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FutileEngine by Matt Rix - 

public class Futile : MonoBehaviour 
{
	static public Futile instance = null;
	
	
	
	static public FScreen screen;
	
	static public FAtlasManager atlasManager;
	
	static public FStage stage;
	
	static public FTouchManager touchManager;
	
	static private List<FStage> _stages;
	
	
	
	static public bool isOpenGL; //assigned in Awake
	
	
	
	
	//These are set in FScreen
	static public float displayScale; //set based on the resolution setting (the unit to pixel scale)
	static public float displayScaleInverse; // 1/displayScale
	
	static public float resourceScale; //set based on the resolution setting (the scale of assets)
	static public float resourceScaleInverse; // 1/resourceScale
	
	static public string resourceSuffix; //set based on the resLevel
	
	
	
	//used by the rendering engine
	internal static int startingQuadsPerLayer;
	internal static int quadsPerLayerExpansion;
	internal static int maxEmptyQuadsPerLayer;	
	
	
	
	public event Action SignalUpdate;
	public event Action SignalLateUpdate;
	
	
	
	
	private GameObject _cameraHolder;
	private Camera _camera;
	

	private FutileParams _futileParams;
	
	// Use this for initialization
	private void Awake () 
	{
		instance = this;
		isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
		enabled = false;
	}
	
	public void Init(FutileParams futileParams)
	{	
		enabled = true;
		_futileParams = futileParams;
		
		Application.targetFrameRate = _futileParams.targetFrameRate;
		
		FShader.Init(); //set up the basic shaders
		
		Futile.startingQuadsPerLayer = _futileParams.startingQuadsPerLayer;
		Futile.quadsPerLayerExpansion = _futileParams.quadsPerLayerExpansion;
		Futile.maxEmptyQuadsPerLayer = _futileParams.maxEmptyQuadsPerLayer;
		
		screen = new FScreen(_futileParams);
		
		//
		//Camera setup from https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UI.cs
		//
				
		name = "Futile"; 
		
		_cameraHolder = new GameObject();
		_cameraHolder.transform.parent = gameObject.transform;
		
		_camera = _cameraHolder.AddComponent<Camera>();
		_camera.name = "FCamera";
		//_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera.clearFlags = CameraClearFlags.SolidColor;
		_camera.nearClipPlane = -50.3f;
		_camera.farClipPlane = 50.0f;
		_camera.depth = 100;
		_camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		_camera.backgroundColor = _futileParams.backgroundColor;
		
		//we multiply this stuff by scaleInverse to make sure everything is in points, not pixels
		_camera.orthographic = true;
		_camera.orthographicSize = screen.pixelHeight/2 * displayScaleInverse;

		UpdateCameraPosition();
		
		
		
		touchManager = new FTouchManager();
		
		atlasManager = new FAtlasManager();
		
		_stages = new List<FStage>();
		
		stage = new FStage("Futile.stage");
		
		_stages.Add (stage);
	}

	
	
	private void Update()
	{
		screen.Update();

		touchManager.Update();
		if(SignalUpdate != null) SignalUpdate();
		stage.Redraw (false,false);
	}
	
	private void LateUpdate()
	{
		stage.LateUpdate();
		if(SignalLateUpdate != null) SignalLateUpdate();
	}	
	
	private void OnApplicationQuit()
	{
		instance = null;
	}
	
	private void OnDestroy()
	{
		instance = null;	
	}
	
	public void UpdateCameraPosition()
	{
		_camera.orthographicSize = screen.pixelHeight/2 * displayScaleInverse;
		
		float camXOffset = ((screen.originX - 0.5f) * -screen.pixelWidth)*displayScaleInverse;
		float camYOffset = ((screen.originY - 0.5f) * -screen.pixelHeight)*displayScaleInverse;
	
		_camera.transform.position = new Vector3(camXOffset, camYOffset, -10.0f); 	
	}
	
	//
	//THE MIGHTY LAND OF DEPRECATION
	//

	public bool IsLandscape()
	{
		throw new NotSupportedException("Deprecated! Use Futile.screen.IsLandscape() instead");
	}
	
	public float originX
	{
		get {throw new NotSupportedException("Deprecated! Use Futile.screen.originX instead");}
		set {throw new NotSupportedException("Deprecated! Use Futile.screen.originX instead");}
	}

	public float originY
	{
		get {throw new NotSupportedException("Deprecated! Use Futile.screen.originY instead"); }
		set {throw new NotSupportedException("Deprecated! Use Futile.screen.originY instead");}
	}
	
	public ScreenOrientation currentOrientation
	{
		get {throw new NotSupportedException("Deprecated! Use Futile.screen.currentOrientation instead");}
		set {throw new NotSupportedException("Deprecated! Use Futile.screen.currentOrientation instead");}
	}
	
	static public float width
	{
		get {throw new NotSupportedException("Deprecated! Use Futile.screen.width instead");}
		set {throw new NotSupportedException("Deprecated! Use Futile.screen.width instead");}
	}
	
	static public float height
	{
		get {throw new NotSupportedException("Deprecated! Use Futile.screen.height instead");}
		set {throw new NotSupportedException("Deprecated! Use Futile.screen.height instead");}
	}
	

}

