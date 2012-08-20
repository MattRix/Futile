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

	
	
	static public bool isOpenGL; //assigned in Awake
	
	
	
	
	//These are set in FScreen
	static public float displayScale; //set based on the resolution setting (the unit to pixel scale)
	static public float displayScaleInverse; // 1/displayScale
	
	static public float resourceScale; //set based on the resolution setting (the scale of assets)
	static public float resourceScaleInverse; // 1/resourceScale
	
	static public string resourceSuffix; //set based on the resLevel
	
	
	
	//used by the rendering engine
	static internal int startingQuadsPerLayer;
	static internal int quadsPerLayerExpansion;
	static internal int maxEmptyQuadsPerLayer;	
	
	static internal int nextRenderLayerDepth = 0;
	
	
	static private List<FStage> _stages;
	static private bool _isDepthChangeNeeded = false;
	
	
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
		name = "Futile";
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
		
		_cameraHolder = new GameObject();
		_cameraHolder.transform.parent = gameObject.transform;
		
		_camera = _cameraHolder.AddComponent<Camera>();
		_camera.name = "Camera";
		//_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera.clearFlags = CameraClearFlags.SolidColor;
		_camera.nearClipPlane = -50.0f;
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
		
		AddStage (stage);
	}
	
	static public void AddStage(FStage stageToAdd)
	{
		int stageIndex = _stages.IndexOf(stageToAdd);
		
		if(stageIndex == -1) //add it if it's not a stage
		{
			stageToAdd.HandleAddedToFutile();
			_stages.Add(stageToAdd);
			UpdateStageIndices();
		}
		else if(stageIndex != _stages.Count-1) //if stage is already in the stages, put it at the top of the stages if it's not already
		{
			_stages.RemoveAt(stageIndex);
			_stages.Add(stageToAdd);
			UpdateStageIndices();
		}
	}
	
	static public void AddStageAtIndex(FStage stageToAdd, int newIndex)
	{
		int stageIndex = _stages.IndexOf(stageToAdd);
		
		if(newIndex > _stages.Count) //if it's past the end, make it at the end
		{
			newIndex = _stages.Count;
		}
		
		if(stageIndex == newIndex) return; //if it's already at the right index, just leave it there
		
		if(stageIndex == -1) //add it if it's not in the stages already
		{
			stageToAdd.HandleAddedToFutile();
			
			_stages.Insert(newIndex, stageToAdd);
		}
		else //if stage is already in the stages, move it to the desired index
		{
			_stages.RemoveAt(stageIndex);
			
			if(stageIndex < newIndex)
			{
				_stages.Insert(newIndex-1, stageToAdd); //gotta subtract 1 to account for it moving in the order
			}
			else
			{
				_stages.Insert(newIndex, stageToAdd);
			}
		}
		
		UpdateStageIndices();
	}
	
	static public void RemoveStage(FStage stageToRemove)
	{
		stageToRemove.HandleRemovedFromFutile();
		stageToRemove.index = -1;
		
		_stages.Remove(stageToRemove);
		
		UpdateStageIndices();
	}

	static public void UpdateStageIndices ()
	{
		int stageCount = _stages.Count;
		for(int s = 0; s<stageCount; s++)
		{
			_stages[s].index = s;	
		}
		
		_isDepthChangeNeeded = true;
	}
	
	static public int GetStageCount()
	{
		return _stages.Count;
	}
	
	static public FStage GetStageAt(int index)
	{
		return _stages[index];
	}

	
	
	private void Update()
	{
		screen.Update();

		touchManager.Update();
		if(SignalUpdate != null) SignalUpdate();
		
		
		for(int s = 0; s<_stages.Count; s++)
		{
			_stages[s].Redraw (false,_isDepthChangeNeeded);
		}
		
		_isDepthChangeNeeded = false;
	}
	
	private void LateUpdate()
	{
		nextRenderLayerDepth = 0;
		
		for(int s = 0; s<_stages.Count; s++)
		{
			_stages[s].LateUpdate();
		}
		
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
	
	new public Camera camera
	{
		get {return _camera;}	
	}
	
	//
	//THE MIGHTY LAND OF DEPRECATION
	//
	
	[Obsolete("Futile.IsLandscape() is obsolete, use Futile.screen.IsLandscape() instead")]
	public bool IsLandscape()
	{
		throw new NotSupportedException("Obsolete! Use Futile.screen.IsLandscape() instead");
	}
	
	[Obsolete("Futile.originX is obsolete, use Futile.screen.originX instead")]
	public float originX
	{
		get {throw new NotSupportedException("Obsolete! Use Futile.screen.originX instead");}
		set {throw new NotSupportedException("Obsolete! Use Futile.screen.originX instead");}
	}
	
	[Obsolete("Futile.originY is obsolete, use Futile.screen.originY instead")]
	public float originY
	{
		get {throw new NotSupportedException("Obsolete! Use Futile.screen.originY instead"); }
		set {throw new NotSupportedException("Obsolete! Use Futile.screen.originY instead");}
	}
	
	[Obsolete("Futile.currentOrientation is obsolete, use Futile.screen.currentOrientation instead")]
	public ScreenOrientation currentOrientation
	{
		get {throw new NotSupportedException("Obsolete! Use Futile.screen.currentOrientation instead");}
		set {throw new NotSupportedException("Obsolete! Use Futile.screen.currentOrientation instead");}
	}
	
	[Obsolete("Futile.width is obsolete, use Futile.screen.width instead")]
	static public float width
	{
		get {throw new NotSupportedException("Obsolete! Use Futile.screen.width instead");}
		set {throw new NotSupportedException("Obsolete! Use Futile.screen.width instead");}
	}
	
	[Obsolete("Futile.height is obsolete, use Futile.screen.height instead")]
	static public float height
	{
		get {throw new NotSupportedException("Obsolete! Use Futile.screen.height instead");}
		set {throw new NotSupportedException("Obsolete! Use Futile.screen.height instead");}
	}
	

}

