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
	protected Rect _boundsRect;

	protected bool _isMeshDirty = false;
	protected bool _areLocalVerticesDirty = false;
	
	public FSprite (string elementName) : base()
	{
		Init(Futile.atlasManager.GetElementWithName(elementName),1);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	public void SetElementByName(string elementName)
	{
		this.element = Futile.atlasManager.GetElementWithName(elementName);
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
			_alphaColor = _color.CloneWithMultipliedAlpha(_concatenatedAlpha);	
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
		
		_boundsRect.width = _element.sourceSize.x;
		_boundsRect.height = _element.sourceSize.y;
		_boundsRect.x = -_anchorX*_boundsRect.width;
		_boundsRect.y = -_anchorY*_boundsRect.height;
		
		float sourceWidth = _element.sourceRect.width;
		float sourceHeight = _element.sourceRect.height;
		float left = _boundsRect.x + _element.sourceRect.x;
		float bottom = _boundsRect.y + (_boundsRect.height - _element.sourceRect.y - _element.sourceRect.height);
		
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
			
			int vertexIndex = _firstQuadIndex*4;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex], _localVertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 1], _localVertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 2], _localVertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 3], _localVertices[3],0);
			
			uvs[vertexIndex] = _element.uvTopLeft;
			uvs[vertexIndex + 1] = _element.uvTopRight;
			uvs[vertexIndex + 2] = _element.uvBottomRight;
			uvs[vertexIndex + 3] = _element.uvBottomLeft;
			
			colors[vertexIndex] = _alphaColor;
			colors[vertexIndex + 1] = _alphaColor;
			colors[vertexIndex + 2] = _alphaColor;
			colors[vertexIndex + 3] = _alphaColor;
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	virtual public Rect boundsRect
	{
		get {return _boundsRect;}	
	}
	
	virtual public Rect localRect
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
		get { return _scaleX * _boundsRect.width; }
		set { _scaleX = value/_boundsRect.width; _isMatrixDirty = true; } 
	}
	
	virtual public float height
	{
		get { return _scaleY * _boundsRect.height; }
		set { _scaleY = value/_boundsRect.height; _isMatrixDirty = true; } 
	}
	
	virtual public float anchorX 
	{
		get { return _anchorX;}
		set { _anchorX = value; _areLocalVerticesDirty = true; }
	}
	
	virtual public float anchorY 
	{
		get { return _anchorY;}
		set { _anchorY = value; _areLocalVerticesDirty = true; }
	}
}

