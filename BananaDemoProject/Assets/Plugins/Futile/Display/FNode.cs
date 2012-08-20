using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FNode
{
	protected float _x;
	protected float _y;
	protected float _scaleX;
	protected float _scaleY;
	protected float _rotation;
	
	protected float _sortZ; //sortZ is used for depth sorting but ONLY if the node container's shouldSortByZ = true;
	
	protected bool _isMatrixDirty;
	
	protected FContainer _container = null;
	
	protected FMatrix _matrix;
	protected FMatrix _concatenatedMatrix;
	protected FMatrix _inverseConcatenatedMatrix = null;

	protected FMatrix _screenConcatenatedMatrix = null;
	protected FMatrix _screenInverseConcatenatedMatrix = null;
	
	protected bool _needsSpecialMatrices = false;
	
	
	protected float _alpha;
	protected float _concatenatedAlpha;
	protected bool _isAlphaDirty;
	
	protected bool _isOnStage = false;
	
	protected int _depth;
	
	protected FStage _stage = null; //assigned in HandleAddedToStage
	
	protected bool _isVisible = true;
	protected float _visibleAlpha = 1.0f;
	
	public object data = null; //the user can put whatever data they want here
	
	public FNode () 
	{
		_depth = 0;
		
		_x = 0;
		_y = 0;
		_scaleX = 1;
		_scaleY = 1;
		_rotation = 0;
		
		_sortZ = 0;
		
		_alpha = 1.0f;
		_concatenatedAlpha = 1.0f;
		_isAlphaDirty = false;
		
		_matrix = new FMatrix();
		_concatenatedMatrix = new FMatrix();
		_isMatrixDirty = false;
		
	}
	
	public Vector2 LocalToStage(Vector2 localVector)
	{
		return _concatenatedMatrix.GetNewTransformedVector(localVector);
	}
	
	public Vector2 StageToLocal(Vector2 globalVector) 
	{
		//using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
		return this.inverseConcatenatedMatrix.GetNewTransformedVector(globalVector);
	}
	
	public Vector2 LocalToGlobal(Vector2 localVector)
	{
		//using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
		return this.screenConcatenatedMatrix.GetNewTransformedVector(localVector);
	}
	
	public Vector2 GlobalToLocal(Vector2 globalVector)
	{
		//using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
		return this.screenInverseConcatenatedMatrix.GetNewTransformedVector(globalVector);
	}
	
	public Vector2 LocalToLocal(FNode otherNode, Vector2 otherVector) //returns the position in THIS node of a point in the OTHER node 
	{
		return GlobalToLocal(otherNode.LocalToGlobal(otherVector));
	}
	
	public void UpdateMatrix()
	{
		if(!_isMatrixDirty) return;
		
		_isMatrixDirty = false;
		
		_matrix.SetScaleThenRotate(_x,_y,_scaleX,_scaleY,_rotation * -RXMath.DTOR);
			
		if(_container != null)
		{
			_concatenatedMatrix.ConcatAndCopyValues(_matrix, _container.concatenatedMatrix);
		}
		else
		{
			_concatenatedMatrix.CopyValues(_matrix);	
		}
		
		if(_needsSpecialMatrices)
		{
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
			_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
		}
	}
	
	virtual protected void UpdateDepthMatrixAlpha(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if(shouldUpdateDepth)
		{
			_depth = _stage.nextNodeDepth++;	
		}
		
		if(_isMatrixDirty || shouldForceDirty)
		{
			_isMatrixDirty = false;
			
			_matrix.SetScaleThenRotate(_x,_y,_scaleX,_scaleY,_rotation * -RXMath.DTOR);
			
			if(_container != null)
			{
				_concatenatedMatrix.ConcatAndCopyValues(_matrix, _container.concatenatedMatrix);
			}
			else
			{
				_concatenatedMatrix.CopyValues(_matrix);	
			}
		}
		
		if(_needsSpecialMatrices)
		{
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
			_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
		}
		
		if(_isAlphaDirty || shouldForceDirty)
		{
			_isAlphaDirty = false;
			
			if(_container != null)
			{
				_concatenatedAlpha = _container.concatenatedAlpha*_alpha*_visibleAlpha;
			}
			else 
			{
				_concatenatedAlpha = _alpha*_visibleAlpha;
			}
		}	
	}
	
	virtual public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{	
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
	}
	
	virtual public void HandleAddedToStage()
	{
		_isOnStage = true;
	}

	virtual public void HandleRemovedFromStage()
	{
		_isOnStage = false;
	} 
	
	virtual public void HandleAddedToContainer(FContainer container)
	{
		if(_container != container)
		{
			if(_container != null) //remove from the old container first if there is one
			{
				_container.RemoveChild(this);	
			}
			
			_container = container;
		}
	}
	
	virtual public void HandleRemovedFromContainer()
	{
		_container = null;	
	}
	
	public void RemoveFromContainer()
	{
		if(_container != null) _container.RemoveChild (this);
	}
	
	public void MoveToTop()
	{
		if(_container != null) _container.AddChild(this);
	}
	
	public void MoveToBottom()
	{
		if(_container != null) _container.AddChildAtIndex(this,0);	
	}
	
	public bool isVisible
	{
		get { return _isVisible;}
		set 
		{ 
			if(_isVisible != value)
			{
				_isVisible = value;
				_visibleAlpha = _isVisible ? 1.0f : 0.0f;
				_isAlphaDirty = true;
			}
		}
	}
	
	public float x
	{
		get { return _x;}
		set { _x = value; _isMatrixDirty = true;}
	}
	
	public float y
	{
		get { return _y;}
		set { _y = value; _isMatrixDirty = true;}
	}
	
	virtual public float sortZ
	{
		get { return _sortZ;}
		set { _sortZ = value;} 
	}
	
	public float scaleX
	{
		get { return _scaleX;}
		set { _scaleX = value; _isMatrixDirty = true;}
	}
	
	public float scaleY
	{
		get { return _scaleY;}
		set { _scaleY = value; _isMatrixDirty = true;}
	}
	
	public float scale
	{
		get { return scaleX;} //return scaleX because if we're using this, x and y should be the same anyway
		set { _scaleX = value; scaleY = value; _isMatrixDirty = true;}
	}
	
	public float rotation
	{
		get { return _rotation;}
		set { _rotation = value; _isMatrixDirty = true;}
	}
	
	public bool isMatrixDirty
	{
		get { return _isMatrixDirty;}	
	}
	
	public FContainer container
	{
		get { return _container;}	
	}
	
	public int depth
	{
		get { return _depth;}	
	}
	
	virtual public int touchPriority
	{
		get { return _depth;}	
	}
	
	virtual public FMatrix matrix 
	{
		get { return _matrix; }
	}
	
	virtual public FMatrix concatenatedMatrix 
	{
		get { return _concatenatedMatrix; }
	}
	
	protected void CreateSpecialMatrices() 
	{
		_needsSpecialMatrices = true; //after now the matrices will be updated on redraw	 
		
		_inverseConcatenatedMatrix = new FMatrix();
		_screenConcatenatedMatrix = new FMatrix();
		_screenInverseConcatenatedMatrix = new FMatrix();
		
		_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
		_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
		_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
	}
	
	virtual public FMatrix inverseConcatenatedMatrix 
	{
		get 
		{ 
			if(!_needsSpecialMatrices) CreateSpecialMatrices(); //only it create if needed
			
			return _inverseConcatenatedMatrix; 
		}
	}
	
	virtual public FMatrix screenConcatenatedMatrix 
	{
		get 
		{ 
			if(!_needsSpecialMatrices) CreateSpecialMatrices(); //only it create if needed
			
			return _screenConcatenatedMatrix; 
		}
	}
	
	virtual public FMatrix screenInverseConcatenatedMatrix 
	{
		get 
		{ 
			if(!_needsSpecialMatrices) CreateSpecialMatrices(); //only it create if needed

			return _screenInverseConcatenatedMatrix; 
		}
	}
	
	public float alpha 
	{
		get { return _alpha; }
		set 
		{ 
			float newAlpha = Math.Max (0.0f, Math.Min (1.0f, value));
			if(_alpha != newAlpha)
			{
				_alpha = newAlpha; 
				_isAlphaDirty = true;
			}
		}
	}
	
	public float concatenatedAlpha 
	{
		get { return _concatenatedAlpha; }
	}
	
	public FStage stage
	{
		get {return _stage;}
		set {_stage = value;}
	}
	
}

