using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FacileEngine by Matt Rix - 

public class FEngine : MonoBehaviour 
{
	static public FEngine instance = null;
	
	static public FAtlasManager atlasManager;
	
	static public FStage stage;
	
	static public FTouchManager touchManager;
	
	static public bool isOpenGL; //assigned in Awake
	
	static public int scale; //set based on real screen width
	static public float scaleInverse; //float which is 1/scale
	
	static public float width; //in points, not pixels
	static public float height; //in points, not pixels
	
	static public float halfWidth; //in points
	static public float halfHeight; //in points
	
	public int drawDepth = 100;
	
	private GameObject _cameraHolder;
	private Camera _camera;
	
	public int targetFrameRate = 60;
	
	//anchor 0,0 sets coord 0,0 at bottom left
	//anchor 0.5f,0.5f sets coord 0,0 at center
	private float _cameraAnchorX = 0.5f;
	private float _cameraAnchorY = 0.5f;
	
	public static int startingQuadsPerLayer;
	public static int quadsPerLayerExpansion;

	// Use this for initialization
	private void Awake () 
	{
		instance = this;
		isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
	}
	
	public void Init(int startingQuadsPerLayer, int quadsPerLayerExpansion)
	{
		Application.targetFrameRate = targetFrameRate;
		
		FEngine.startingQuadsPerLayer = startingQuadsPerLayer;
		FEngine.quadsPerLayerExpansion = quadsPerLayerExpansion;
		
		//FTouchManager.instance = new FTouchManager();
		
		//widths based on landscape
		
		
		if(Screen.width <= 480.0f)
		{
			scale = 1;
		}
		else if (Screen.width <= 1024.0f)
		{
			scale = 2;
		}
		else
		{
			scale = 4;
		}
		
		scaleInverse = 1.0f/(float)scale;
		
		width = Screen.width*scaleInverse;
		height = Screen.height*scaleInverse;
		
		halfWidth = width/2;
		halfHeight = height/2;
		
		Debug.Log ("FEngine: Scale is " + scale);
		
		Debug.Log ("FEngine: Screen size in pixels is (" + Screen.width +"," + Screen.height+")");
		
		Debug.Log ("FEngine: Screen size in points is (" + width + "," + height+")");
		
		//
		//Camera setup from https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UI.cs
		//
				
		name = "FEngine"; 
		
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
		_camera.orthographicSize = Screen.height/2 * scaleInverse;
		//_camera.transform.position = new Vector3(Screen.width/2 * scaleInverse, Screen.height/2 * scaleInverse , -10.0f);
		//_camera.transform.position = new Vector3(0, 0 , -10.0f); //center the screen
		
		float camXOffset = ((_cameraAnchorX - 0.5f) * -Screen.width)*scaleInverse;
		float camYOffset = ((_cameraAnchorY - 0.5f) * -Screen.height)*scaleInverse;
	
		_camera.transform.position = new Vector3(camXOffset, camYOffset, -10.0f); 
		
		touchManager = new FTouchManager();
		
		atlasManager = new FAtlasManager();
		
		stage = new FStage();
	}
	
	protected void Update()
	{
		touchManager.Update();
		stage.Update (false,false);
	}
	
	protected void LateUpdate()
	{
		stage.LateUpdate();
	}	
	
	protected void OnApplicationQuit()
	{
		instance = null;
	}
	
	protected void OnDestroy()
	{
		instance = null;	
	}

	public float cameraAnchorX
	{
		get {return _cameraAnchorX;}
		//set {_cameraAnchorX = value;}
	}

	public float cameraAnchorY
	{
		get {return _cameraAnchorY;}
		//set {_cameraAnchorY = value;}
	}
	

}
