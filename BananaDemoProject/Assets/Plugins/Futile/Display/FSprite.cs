using UnityEngine;
using System;

public class FSprite : FQuadNode
{
	protected Color _color = Color.white;
	protected Color _alphaColor = Color.white;
	
	protected Vector2[] _localVertices = new Vector2[4];
	
	protected float _anchorX = 0.5f;
	protected float _anchorY = 0.5f;
	
	protected Rect _localRect;
	protected Rect _textureRect;

	protected bool _isMeshDirty = false;
	protected bool _areLocalVerticesDirty = false;
	
	protected FSprite() : base() //for overriding
	{
		
	}
	
	public FSprite (string elementName) : base()
	{
		Init(Futile.atlasManager.GetElementWithName(elementName),1);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	
	override public void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(shouldUpdateDepth)
		{
			UpdateQuads();
		}
		
		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		
		if(wasAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_color.ApplyMultipliedAlpha(ref _alphaColor, _concatenatedAlpha);	
		}
		
		if(_areLocalVerticesDirty)
		{
			UpdateLocalVertices();
		}
		
		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}
	
	virtual protected void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		
		_textureRect.width = _element.sourceSize.x;
		_textureRect.height = _element.sourceSize.y;
		_textureRect.x = -_anchorX*_textureRect.width;
		_textureRect.y = -_anchorY*_textureRect.height;
		
		float sourceWidth = _element.sourceRect.width;
		float sourceHeight = _element.sourceRect.height;
		float left = _textureRect.x + _element.sourceRect.x;
		float bottom = _textureRect.y + (_textureRect.height - _element.sourceRect.y - _element.sourceRect.height);
		
		_localRect.x = left;
		_localRect.y = bottom;
		_localRect.width = sourceWidth;
		_localRect.height = sourceHeight;
		
		_localVertices[0].Set(left,bottom + sourceHeight);
		_localVertices[1].Set(left + sourceWidth,bottom + sourceHeight);
		_localVertices[2].Set(left + sourceWidth,bottom);
		_localVertices[3].Set(left,bottom);
		
		_isMeshDirty = true;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstQuadIndex != -1) 
		{
			_isMeshDirty = false;
			
			int vertexIndex0 = _firstQuadIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _localVertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], _localVertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], _localVertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], _localVertices[3],0);
			
			uvs[vertexIndex0] = _element.uvTopLeft;
			uvs[vertexIndex1] = _element.uvTopRight;
			uvs[vertexIndex2] = _element.uvBottomRight;
			uvs[vertexIndex3] = _element.uvBottomLeft;
			
			colors[vertexIndex0] = _alphaColor;
			colors[vertexIndex1] = _alphaColor;
			colors[vertexIndex2] = _alphaColor;
			colors[vertexIndex3] = _alphaColor;
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	virtual public Rect textureRect //the full rect as if the sprite hadn't been trimmed
	{
		get {return _textureRect;}	
	}
	
	[Obsolete("FSprite's boundsRect is obsolete, use textureRect instead")]
	public Rect boundsRect
	{
		get {throw new NotSupportedException("boundsRect is obsolete! Use textureRect instead");}
	}
	
	virtual public Rect localRect //the rect of the actual trimmed quad drawn on screen
	{
		get {return _localRect;}	
	}

	virtual public Color color 
	{
		get { return _color; }
		set 
		{ 
			if(_color != value)
			{
				_color = value; 
				_isAlphaDirty = true;
			}
		}
	}
	
	virtual public float width
	{
		get { return _scaleX * _textureRect.width; }
		set { _scaleX = value/_textureRect.width; _isMatrixDirty = true; } 
	}
	
	virtual public float height
	{
		get { return _scaleY * _textureRect.height; }
		set { _scaleY = value/_textureRect.height; _isMatrixDirty = true; } 
	}
	
	virtual public float anchorX 
	{
		get { return _anchorX;}
		set 
		{ 
			if(_anchorX != value)
			{
				_anchorX = value; 
				_areLocalVerticesDirty = true; 
			}
		}
	}
	
	virtual public float anchorY 
	{
		get { return _anchorY;}
		set 
		{ 
			if(_anchorY != value)
			{
				_anchorY = value; 
				_areLocalVerticesDirty = true; 
			}
		}
	}
}

