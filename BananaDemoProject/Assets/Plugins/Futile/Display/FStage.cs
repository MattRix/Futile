using UnityEngine;
using System.Collections;

public class FStage : FContainer
{
	public int nextNodeDepth;
	public int index;
	
	private bool _needsDepthUpdate = false;
	
	private FRenderer _renderer;
	
	private string _name;
	
	private FMatrix _identityMatrix;
	
	private bool _doesRendererNeedTransformChange = false;
	
	public FStage(string name) : base()
	{
		_name = name;
		
		_stage = this;
		
		_renderer = new FRenderer(this);
		
		_identityMatrix = new FMatrix();
		_identityMatrix.Identity();
		
		_inverseConcatenatedMatrix = new FMatrix();
		_screenConcatenatedMatrix = new FMatrix();
		_screenInverseConcatenatedMatrix = new FMatrix();
		
		HandleAddedToStage(); //add it to itself!
	}

	public void HandleQuadsChanged ()
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
			
			_matrix.SetScaleThenRotate(_x,_y,_scaleX,_scaleY,_rotation * -RXMath.DTOR);
			_concatenatedMatrix.CopyValues(_matrix);	
			
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			
			_doesRendererNeedTransformChange = true;
		}
		
		if(_isAlphaDirty || shouldForceDirty)
		{
			_isAlphaDirty = false;
			
			_concatenatedAlpha = _alpha*_visibleAlpha;
		}	
	}
		
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool didNeedDepthUpdate = _needsDepthUpdate;
		
		_needsDepthUpdate = false;
		
		if(didNeedDepthUpdate)
		{
			shouldForceDirty = true;
			shouldUpdateDepth = true;
			nextNodeDepth = index*1000;
			_renderer.StartRender(); 
		}
		
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		foreach(FNode node in _childNodes)
		{
			//key difference between Stage and Container: Stage doesn't force dirty if matrix is dirty
			node.Redraw(shouldForceDirty || wasAlphaDirty, shouldUpdateDepth); //if the matrix was dirty or we're supposed to force it, do it!
		}
		
		if(didNeedDepthUpdate)
		{
			_renderer.EndRender();
			Futile.touchManager.HandleDepthChange(); 
		}
		
		if(_doesRendererNeedTransformChange)
		{
			_doesRendererNeedTransformChange = false;
			
			_renderer.SetTransformForLayers
			(
				new Vector3(_x,_y,0),
				Quaternion.AngleAxis(_rotation,Vector3.back),
				new Vector3(_scaleX, _scaleY, 1.0f)
			);
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
	
}
