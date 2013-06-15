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
	
	static public int baseRenderQueueDepth = 3000;
	
	static public bool shouldRemoveAtlasElementFileExtensions = true;
	
	
	//These are set in FScreen
	static public float displayScale; //set based on the resolution setting (the unit to pixel scale)
	static public float displayScaleInverse; // 1/displayScale
	
	static public float resourceScale; //set based on the resolution setting (the scale of assets)
	static public float resourceScaleInverse; // 1/resourceScale
	
	static public float screenPixelOffset; //set based on whether it's openGL or not
	
	static public string resourceSuffix; //set based on the resLevel
	
	//default element, a 16x16 white texture
	static public FAtlasElement whiteElement;
	static public Color white = Color.white; //unlike Futile.white, it doesn't create a new color every time
	
	static internal int nextRenderLayerDepth = 0;
	
	
	static private List<FStage> _stages;
	static private bool _isDepthChangeNeeded = false;
	
	public delegate void FutileUpdateDelegate();
	
	public event FutileUpdateDelegate SignalUpdate;
	public event FutileUpdateDelegate SignalFixedUpdate;
	public event FutileUpdateDelegate SignalLateUpdate;
    
	//configuration values
	public bool shouldTrackNodesInRXProfiler = true;
    
    private GameObject _cameraHolder;
    private Camera _camera;

	private bool _shouldRunGCNextUpdate = false; //use Futile.instance.ForceGarbageCollectionNextUpdate();

    private FutileParams _futileParams;
    
    private List<FDelayedCallback> _delayedCallbacks = new List<FDelayedCallback>();


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
		FFacetType.Init(); //set up the types of facets (Quads, Triangles, etc)
		
		screen = new FScreen(_futileParams);
		
		//
		//Camera setup from https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UI.cs
		//
		
		_cameraHolder = new GameObject();
		_cameraHolder.transform.parent = gameObject.transform;
		
		_camera = _cameraHolder.AddComponent<Camera>();
		_camera.tag = "MainCamera";
		_camera.name = "Camera";
		//_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera.clearFlags = CameraClearFlags.SolidColor;
		_camera.nearClipPlane = 0.0f;
		_camera.farClipPlane = 500.0f;
		_camera.depth = 100;
		_camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		_camera.backgroundColor = _futileParams.backgroundColor;
		
		//we multiply this stuff by scaleInverse to make sure everything is in points, not pixels
		_camera.orthographic = true;
		_camera.orthographicSize = screen.pixelHeight/2 * displayScaleInverse;

		UpdateCameraPosition();
		
		touchManager = new FTouchManager();
		
		atlasManager = new FAtlasManager();
		
		CreateDefaultAtlases();
		
		
		_stages = new List<FStage>();
		
		stage = new FStage("Futile.stage");
		
		AddStage (stage);
	}

    public FDelayedCallback StartDelayedCallback(Action func, float delayTime)
    {
        if (delayTime <= 0) delayTime = 0.00001f; //super small delay for 0 to avoid divide by 0 errors

        FDelayedCallback callback = new FDelayedCallback(func, delayTime);
        _delayedCallbacks.Add(callback);

        return callback;
    }

    public void StopDelayedCall(Action func)
    {
        int count = _delayedCallbacks.Count;

        for (int d = 0; d<count; d++)
        {
            FDelayedCallback call = _delayedCallbacks[d];

            if(call.func == func)
            {
                _delayedCallbacks.RemoveAt(d);
                d--;
                count--;
            }
        }
    }

    public void StopDelayedCall(FDelayedCallback callToRemove)
    {
        _delayedCallbacks.Remove(callToRemove);
    }

	public void CreateDefaultAtlases()
	{
		//atlas of plain white
		
		Texture2D plainWhiteTex = new Texture2D(16,16);
		plainWhiteTex.filterMode = FilterMode.Bilinear;
		plainWhiteTex.wrapMode = TextureWrapMode.Clamp;
		
		Color white = Futile.white;
		//Color clear = new Color(1,1,1,0);
		
		for(int r = 0; r<16; r++)
		{
			for(int c = 0; c<16; c++)
			{
//				if(c == 0 || r  == 0) //clear the 0 edges
//				{
//					plainWhiteTex.SetPixel(c,r,clear);
//				}
//				else 
//				{
					plainWhiteTex.SetPixel(c,r,white);
//				}
			}
		}
		
		
		plainWhiteTex.Apply();
		
		atlasManager.LoadAtlasFromTexture("Futile_White",plainWhiteTex);
		
		whiteElement = atlasManager.GetElementWithName("Futile_White");
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

    private void ProcessDelayedCallbacks()
    {
        int count = _delayedCallbacks.Count;

        for (int d = 0; d<count; d++)
        {
            FDelayedCallback callback = _delayedCallbacks[d];

            callback.timeRemaining -= Time.deltaTime;

            if(callback.timeRemaining < 0)
            {
                callback.func();
                _delayedCallbacks.RemoveAt(d);
                d--;
                count--;
            }
        }
    }
	
	private void Update()
	{
        ProcessDelayedCallbacks();

		screen.Update();
		
		touchManager.Update();

		if(SignalUpdate != null) SignalUpdate();
		
		for(int s = 0; s<_stages.Count; s++)
		{
			_stages[s].Redraw (false,_isDepthChangeNeeded);
		}
		
		_isDepthChangeNeeded = false;
		
		if(_shouldRunGCNextUpdate)
		{
			_shouldRunGCNextUpdate = false;	
			GC.Collect();
		}
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
	
	private void FixedUpdate()
	{
		if(SignalFixedUpdate != null) SignalFixedUpdate();
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
		
		float camXOffset = ((screen.originX - 0.5f) * -screen.pixelWidth)*displayScaleInverse + screenPixelOffset;
		float camYOffset = ((screen.originY - 0.5f) * -screen.pixelHeight)*displayScaleInverse - screenPixelOffset;
	
		_camera.transform.position = new Vector3(camXOffset, camYOffset, -10.0f); 	
	}

	public void ForceGarbageCollectionNextUpdate()
	{
		_shouldRunGCNextUpdate = true;
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

public class FDelayedCallback
{
    public float delayTime;
    public float timeRemaining;
    public Action func;

    public FDelayedCallback(Action func, float delayTime)
    {
        this.func = func;
        this.delayTime = delayTime;
        this.timeRemaining = delayTime;
    }

    public float percentComplete //0.0f when started, 1.0f when finished
    {
        get 
        {
            return Mathf.Clamp01(1.0f - timeRemaining/delayTime);
        }
    }
}



