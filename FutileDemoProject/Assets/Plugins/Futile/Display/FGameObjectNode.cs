using UnityEngine;
using System;

public class FGameObjectNode : FNode, FRenderableLayerInterface
{
	protected GameObject _gameObject;
	protected bool _shouldLinkPosition;
	protected bool _shouldLinkRotation;
	protected bool _shouldLinkScale;
	protected int _renderQueueDepth = -1;
	
	public bool shouldDestroyOnRemoveFromStage = true;
	
	public FGameObjectNode (GameObject gameObject, bool shouldLinkPosition, bool shouldLinkRotation, bool shouldLinkScale)
	{
		Init (gameObject,shouldLinkPosition,shouldLinkRotation, shouldLinkScale);
	}
	
	protected FGameObjectNode() //for easy overriding
	{
		
	}
	
	protected void Init(GameObject gameObject, bool shouldLinkPosition, bool shouldLinkRotation, bool shouldLinkScale)
	{
		_gameObject = gameObject;
		_shouldLinkPosition = shouldLinkPosition;
		_shouldLinkRotation = shouldLinkRotation;
		_shouldLinkScale = shouldLinkScale;
		
		Setup();
	}
	
	protected void Setup()
	{
		if(_isOnStage)
		{
			_gameObject.transform.parent = Futile.instance.gameObject.transform;
			
			if(_gameObject.renderer != null && _gameObject.renderer.material != null)
			{
				_gameObject.renderer.material.renderQueue = _renderQueueDepth;
			}
		}
		
		UpdateGameObject();
	}
	
	protected void Unsetup()
	{
		_gameObject.transform.parent = null;
	}
	
	override public void HandleAddedToStage()
	{
		if(!_isOnStage)
		{
			base.HandleAddedToStage();
			
			_stage.HandleFacetsChanged();
			_gameObject.transform.parent = Futile.instance.gameObject.transform;
			UpdateGameObject();
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			base.HandleRemovedFromStage();
			
			_gameObject.transform.parent = null;
		
			if(shouldDestroyOnRemoveFromStage)
			{
				UnityEngine.Object.Destroy(_gameObject);	
			}
			
			_stage.HandleFacetsChanged();
		}
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		// Game Object Nodes MUST force themselves to update every frame, otherwise
		// their positions will get out of sync with the stage if the stage moves
		// and no nodes are being added/removed to cause the dirty flag to be set.
		shouldForceDirty = true;

		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		bool needsUpdate = false;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(shouldUpdateDepth)
		{
			needsUpdate = true;
			UpdateDepth();
		}
		
		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			needsUpdate = true;
		}
		
		if(wasAlphaDirty || shouldForceDirty)
		{
			needsUpdate = true;
		}
		
		if(needsUpdate) 
		{
			UpdateGameObject();
		}
	}
		
	protected void UpdateDepth()
	{
		stage.renderer.AddRenderableLayer(this);
	}
	
	virtual public void Update(int depth)
	{
		_renderQueueDepth = Futile.baseRenderQueueDepth+depth;
		
		if(_gameObject.renderer != null && _gameObject.renderer.material != null)
		{
			_gameObject.renderer.material.renderQueue = _renderQueueDepth;
		}
	}
	
	public void UpdateGameObject()
	{
		if(_isOnStage) 
		{
			//TODO: Get these values correctly using the full matrix
			//do it with scale too
			
			//need to use the FULL global matrix
			
			FMatrix matrix = this.screenConcatenatedMatrix;
			
			if(_shouldLinkPosition) _gameObject.transform.localPosition = matrix.GetVector3FromLocalVector2(Vector2.zero,0);
			if(_shouldLinkRotation) _gameObject.transform.eulerAngles = new Vector3(_gameObject.transform.eulerAngles.x,_gameObject.transform.eulerAngles.y,matrix.GetRotation());
			if(_shouldLinkScale) _gameObject.transform.localScale = new Vector3(matrix.GetScaleX(), matrix.GetScaleY (), _gameObject.transform.localScale.z);
		}
	}
	
	public GameObject gameObject
	{
		get {return _gameObject;}
		set 
		{
			if(_gameObject != value)
			{
				Unsetup();
				_gameObject = value;	
				Setup();
			}
		}
	}
	
	public int renderQueueDepth
	{
		get { return _renderQueueDepth;}
	}
	
}


