using UnityEngine;
using System.Collections;
using System;

public class FStageTransform
{
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 localScale;
}

public class FStage : FContainer
{
	public int nextNodeDepth;
	public int index;
	
	private bool _needsDepthUpdate = false;
	
	private FRenderer _renderer;
	
	private string _name;
	
	private FMatrix _identityMatrix;
	
	private bool _doesRendererNeedTransformChange = false;
	
	private FStageTransform _transform = new FStageTransform();
	
	private FMatrix _followMatrix = new FMatrix();
	private FNode _followTarget = null;
	private bool _shouldFollowScale;
	private bool _shouldFollowRotation;

    private int _layer = 0;
	
	public FStage(string name) : base()
	{
		_name = name;
		
		_stage = this;
		
		_renderer = new FRenderer(this);
		
		_identityMatrix = new FMatrix();
		_identityMatrix.ResetToIdentity();
		
		_inverseConcatenatedMatrix = new FMatrix();
		_screenConcatenatedMatrix = new FMatrix();
		_screenInverseConcatenatedMatrix = new FMatrix();
	}

	public void HandleAddedToFutile()
	{
		HandleAddedToStage();
	}
	
	public void HandleRemovedFromFutile()
	{
		_renderer.Clear();
		HandleRemovedFromStage();
	}

	public void HandleFacetsChanged ()
	{
		_needsDepthUpdate = true;
	}
	
	//special implemenation of this because the stage doesn't get transformed normally,
	//instead it transforms the root gameObject that all the renderlayers live on
	override protected void UpdateDepthMatrixAlpha(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if(shouldUpdateDepth)
		{
			_depth = nextNodeDepth++;	
		}
		
		if(_isMatrixDirty || shouldForceDirty)
		{
			_isMatrixDirty = false;
			
			_matrix.SetScaleThenRotate(_x,_y,_scaleX*_visibleScale,_scaleY*_visibleScale,_rotation * -RXMath.DTOR);
			_concatenatedMatrix.CopyValues(_matrix);	
			
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			
			_doesRendererNeedTransformChange = true;
		}
		
		if(_isAlphaDirty || shouldForceDirty)
		{
			_isAlphaDirty = false;
			
			_concatenatedAlpha = _alpha;
		}	
	}
		
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool didNeedDepthUpdate = _needsDepthUpdate || shouldUpdateDepth;
		
		_needsDepthUpdate = false;
		
		if(didNeedDepthUpdate)
		{
			shouldForceDirty = true;
			shouldUpdateDepth = true;
			nextNodeDepth = index*10000; //each stage will be 10000 higher in "depth" than the previous one
			_renderer.StartRender(); 
		}
		
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		int childCount = _childNodes.Count;
		for(int c = 0; c<childCount; c++)
		{
			//key difference between Stage and Container: Stage doesn't force dirty if matrix is dirty
			//in other words, you can move the stage all you want and it won't force its children to redraw
			//this is especially handy for scrolling/zooming purposes
			_childNodes[c].Redraw(shouldForceDirty || wasAlphaDirty, shouldUpdateDepth); //if the alpha is dirty or we're supposed to force it, do it!
		}
		
		UpdateFollow();
		
		if(didNeedDepthUpdate)
		{
			_renderer.EndRender();
			Futile.touchManager.HandleDepthChange(); 
		}
		
		if(_doesRendererNeedTransformChange)
		{
			_doesRendererNeedTransformChange = false;
			
			_transform.position = new Vector3(_x,_y,0);
			_transform.rotation = Quaternion.AngleAxis(_rotation,Vector3.back);
			_transform.localScale = new Vector3(_scaleX*_visibleScale, _scaleX*_visibleScale, _scaleX*_visibleScale); //uniform scale (should be better performance)
			
			_renderer.UpdateLayerTransforms();
		}
	}
	
	public void CenterOn(Vector2 globalPosition)
	{
		if(_followTarget != null) //stop following if we've been told to center
		{
			_followTarget = null;
		}
		
		Vector2 stagePosition = GlobalToLocal(globalPosition);
		
		_followMatrix.SetScaleThenRotate(0,0,_scaleX,_scaleY,_rotation * -RXMath.DTOR);
		
		Vector2 resultPos = _followMatrix.GetNewTransformedVector(stagePosition);
		this.x = -resultPos.x;
		this.y = -resultPos.y;
	}
	
	public void Follow(FNode followTarget, bool shouldFollowScale, bool shouldFollowRotation)
	{
		_followTarget = followTarget;
		_shouldFollowScale = shouldFollowScale;
		_shouldFollowRotation = shouldFollowRotation;
	}
	
	private void UpdateFollow()
	{
		if(_followTarget != null)
		{
			if(_followTarget.stage == null) //the target MUST be on the same stage
			{
				_followTarget = null;
				return; 
			}
			
			if(_shouldFollowScale)
			{
				this.scale = 1.0f/_followTarget.concatenatedMatrix.GetScaleX();
			}
			
			if(_shouldFollowRotation)
			{
				this.rotation = _followTarget.concatenatedMatrix.GetRotation() * RXMath.RTOD;
			}
			
			_followMatrix.SetScaleThenRotate(0,0,_scaleX,_scaleY,_rotation * -RXMath.DTOR);
			
			Vector2 pos = _followMatrix.GetNewTransformedVector(new Vector2(_followTarget.concatenatedMatrix.tx,_followTarget.concatenatedMatrix.ty));
			
			this.x = -pos.x;
			this.y = -pos.y;
		}
	}
	
	public void Unfollow(FNode targetToUnfollow, bool shouldResetPosition) //if null is passed, it'll remove the current target no matter what it is
	{
		if(targetToUnfollow == null || _followTarget == targetToUnfollow)
		{
			_followTarget = null;	
			if(shouldResetPosition) ResetPosition();
		}
	}
	
	public void ResetPosition()
	{
		_x = 0;
		_y = 0;
		_scaleX = 1.0f;
		_scaleY = 1.0f;
		_rotation = 0.0f;
		_isMatrixDirty = true;
	}

    public int layer
    {
        get { return _layer; }
        set
        {
            if(_layer != value)
            {
                _layer = value;
                _doesRendererNeedTransformChange = true;
            }
        }
    }
	
	//notice how we're returning identity matrixes
	//because we don't want our children to think we've been transformed (or else they will transform)
	override public FMatrix matrix
	{
		get {return _identityMatrix;}
	}
	
	override public FMatrix concatenatedMatrix
	{
		get {return _identityMatrix;}
	}
	
	override public FMatrix inverseConcatenatedMatrix
	{
		get {return _identityMatrix;}
	}
	
	//these represent the actual matrix of the stage and therefore the screen
	
	public FMatrix screenMatrix
	{
		get {return _matrix;}
	}
	
	override public FMatrix screenConcatenatedMatrix
	{
		get {return _concatenatedMatrix;}
	}
	
	override public FMatrix screenInverseConcatenatedMatrix
	{
		get {return _inverseConcatenatedMatrix;}
	}
	
	public void LateUpdate() //called by the engine
	{
		_renderer.Update();
	}
	
	public FRenderer renderer
	{
		get {return _renderer;}	
	}
	
	public string name
	{
		get {return _name;}	
	}
	
	public FStageTransform transform
	{
		get {return _transform;}	
	}
	
	new public float scaleX
	{
		get {return _scaleX;}
		set {throw new NotSupportedException("Stage scale must be uniform! Use stage.scale instead");}
	}
	
	new public float scaleY
	{
		get {return _scaleY;}
		set {throw new NotSupportedException("Stage scale must be uniform! Use stage.scale instead");}
	}
	
}
