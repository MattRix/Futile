using UnityEngine;
using System;

public class FSprite : FFacetElementNode
{
	public static float defaultAnchorX = 0.5f;
	public static float defaultAnchorY = 0.5f;
	
	protected Color _color = Futile.white;
	protected Color _alphaColor = Futile.white;
	
	protected Vector2[] _localVertices;
	
	protected float _anchorX = defaultAnchorX;
	protected float _anchorY = defaultAnchorY;
	
	protected Rect _localRect; //localRect is the TRIMMED rect
	protected Rect _textureRect; //textureRect is the UN-TRIMMED rect

	protected bool _isMeshDirty = false;
	protected bool _areLocalVerticesDirty = false;
	
	protected FSprite() : base() //for overriding
	{
		_localVertices = new Vector2[4];
	}
	
	public FSprite (string elementName) : this(Futile.atlasManager.GetElementWithName(elementName))
	{
	}
	
	public FSprite (FAtlasElement element) : base()
	{
		_localVertices = new Vector2[4];
		
		Init(FFacetType.Quad, element,1);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	override public void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
		UpdateLocalVertices();
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(shouldUpdateDepth)
		{
			UpdateFacets();
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
	
	virtual public void UpdateLocalVertices()
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
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			int vertexIndex0 = _firstFacetIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _localVertices[0], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], _localVertices[1], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], _localVertices[2], _meshZ);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], _localVertices[3], _meshZ);
			
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
	
	//Note: this does not consider rotation at all!
	public Rect GetTextureRectRelativeToContainer()
	{
		return _textureRect.CloneAndScaleThenOffset(_scaleX,_scaleY,_x,_y);
	}

	//Note: this does not consider rotation at all!
	public Rect GetLocalRectRelativeToContainer()
	{
		return _localRect.CloneAndScaleThenOffset(_scaleX,_scaleY,_x,_y);
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
	
	//for convenience
	public void SetAnchor(float newX, float newY)
	{
		this.anchorX = newX;
		this.anchorY = newY;
	}
	
	public void SetAnchor(Vector2 newAnchor)
	{
		this.anchorX = newAnchor.x;
		this.anchorY = newAnchor.y;
	}
	
	public Vector2 GetAnchor()
	{
		return new Vector2(_anchorX,_anchorY);	
	}
}

