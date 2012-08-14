using System;
using UnityEngine;

public class FCamera
{
	private float _originX = 0.0f;
	private float _originY = 0.0f;
	
	private float _x = 0.0f;
	private float _y = 0.0f;
	private float _zoom = 1.0f;
	private float _rotation = 0.0f;
	
	public bool doesNeedUpdate = false;
	
	private Camera _camera3D;
	private GameObject _camera3DHolder;
	
	private FContainer _container;
	
	private FMatrix _matrix = new FMatrix();
	private FMatrix _inverseMatrix = new FMatrix();
	
	public FCamera (float originX, float originY)
	{
		_originX = originX;
		_originY = originY;
		
		_camera3DHolder = new GameObject();
		_camera3DHolder.transform.parent = Futile.instance.gameObject.transform;
		
		_camera3D = _camera3DHolder.AddComponent<Camera>();
		
		_camera3D.name = "FCamera";
		//_camera.clearFlags = CameraClearFlags.Depth; //TODO: check if this is faster or not?
		_camera3D.clearFlags = CameraClearFlags.SolidColor;
		_camera3D.nearClipPlane = -50.3f;
		_camera3D.farClipPlane = 50.0f;
		_camera3D.depth = 100;
		_camera3D.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		_camera3D.backgroundColor = Color.black;
		
		//we multiply this stuff by scaleInverse to make sure everything is in points, not pixels
		_camera3D.orthographic = true;
		
		Futile.stage.AddChild(_container = new FContainer()); //a special container that won't move with the camera
		
		Update();
	}
	
	public void Update()
	{
		doesNeedUpdate = false;
		
		float camXOffset = ((_originX - 0.5f) * -Futile.pixelWidth)*Futile.displayScaleInverse + _x;
		float camYOffset = ((_originY - 0.5f) * -Futile.pixelHeight)*Futile.displayScaleInverse + _y;
		
		_matrix.SetScaleThenRotate(camXOffset,camYOffset,1.0f/_zoom,1.0f/_zoom,_rotation * -RXMath.DTOR);
		_inverseMatrix.InvertAndCopyValues(_matrix);
	
		_camera3D.transform.position = new Vector3(camXOffset, camYOffset, -10.0f); 	
		_camera3D.transform.rotation = Quaternion.AngleAxis(_rotation,Vector3.back);
		
		_camera3D.orthographicSize = Futile.pixelHeight/2 * Futile.displayScaleInverse * _zoom;
	}
	
	public float originX
	{
		get {return _originX;}
		set 
		{
			if(_originX != value)
			{
				_originX = value;
				doesNeedUpdate = true;
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
				doesNeedUpdate = true;
			}
		}
	}
	
	public float x
	{
		get {return _x;}
		set 
		{
			if(_x != value)
			{
				_x = value;
				doesNeedUpdate = true;
			}
		}
	}
	
	public float y
	{
		get {return _y;}
		set 
		{
			if(_y != value)
			{
				_y = value;
				doesNeedUpdate = true;
			}
		}
	}
	
	public float zoom
	{
		get {return _zoom;}
		set 
		{
			if(_zoom != value)
			{
				_zoom = value;
				doesNeedUpdate = true;
			}
		}
	}
	
	public float rotation
	{
		get {return _rotation;}
		set 
		{
			if(_rotation != value)
			{
				_rotation = value;
				doesNeedUpdate = true;
			}
		}
	}
	
	public Camera camera3D
	{
		get {return _camera3D;}
	}
	
	public FContainer container
	{
		get {return _container;}	
	}
	
	public FMatrix matrix
	{
		get {return _matrix;}	
	}
	
	public FMatrix inverseMatrix
	{
		get {return _inverseMatrix;}	
	}
}


