using UnityEngine;
using System;

public class FRepeatSprite : FSprite
{
	protected float _width;
	protected float _height;
	
	protected float _scrollX;
	protected float _scrollY;
	
	protected float _textureWidth;
	protected float _textureHeight;
	
	//FRepeatSprite uses a tiling texture
	//Make sure it's using a single image, not an atlas
	//Also make sure the texture is set to a Wrap Mode of "Repeat"
	//You can set scrollX and scrollY to make the texture scroll
	
	public FRepeatSprite (string elementName, float width, float height) : this(elementName,width,height,0,0) {}
	
	public FRepeatSprite (string elementName, float width, float height, float scrollX, float scrollY) : base()
	{
		_width = width;
		_height = height;
		
		_scrollX = scrollX;
		_scrollY = scrollY;
		
		Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(elementName),1);
		
		if(!_element.atlas.isSingleImage)
		{
			throw new FutileException("ScrollingSprite must be used with a single image, not an atlas! Use Futile.atlasManager.LoadImage()");
		}	
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	override public void HandleElementChanged()
	{
		base.HandleElementChanged();
		_textureWidth = _element.atlas.textureSize.x * Futile.resourceScaleInverse;
		_textureHeight = _element.atlas.textureSize.y * Futile.resourceScaleInverse;

		_areLocalVerticesDirty = true;
		UpdateLocalVertices();
	}
	
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		
		_textureRect.width = _width;
		_textureRect.height = _height;
		_textureRect.x = -_anchorX*_width;
		_textureRect.y = -_anchorY*_height;

		_localRect = _textureRect;
			
		_localVertices[0].Set(_textureRect.xMin,_textureRect.yMax);
		_localVertices[1].Set(_textureRect.xMax,_textureRect.yMax);
		_localVertices[2].Set(_textureRect.xMax,_textureRect.yMin);
		_localVertices[3].Set(_textureRect.xMin,_textureRect.yMin);
		
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
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _localVertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], _localVertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], _localVertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], _localVertices[3],0);
			
			//this uv shifting is where the magic happens!
			uvs[vertexIndex0] = new Vector2(_scrollX/_textureWidth, _scrollY/_textureHeight + _height/_textureHeight);
			uvs[vertexIndex1] = new Vector2(_scrollX/_textureWidth + _width/_textureWidth, _scrollY/_textureHeight + _height/_textureHeight);
			uvs[vertexIndex2] = new Vector2(_scrollX/_textureWidth + _width/_textureWidth, _scrollY/_textureHeight);
			uvs[vertexIndex3] = new Vector2(_scrollX/_textureWidth, _scrollY/_textureHeight);
			
			colors[vertexIndex0] = _alphaColor;
			colors[vertexIndex1] = _alphaColor;
			colors[vertexIndex2] = _alphaColor;
			colors[vertexIndex3] = _alphaColor;
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	
	override public Rect textureRect //the full rect as if the sprite hadn't been trimmed
	{
		get {return _textureRect;}	
	}
	
	override public Rect localRect //the rect of the actual trimmed quad drawn on screen
	{
		get {return _localRect;}	
	}
	
	override public float width
	{
		get { return _width; }
		set 
		{ 
			if(_width != value)
			{
				_width = value; 
				_areLocalVerticesDirty = true; 
			}
		} 
	}
	
	override public float height
	{
		get { return _height;}
		set 
		{ 
			if(_height != value)
			{
				_height = value; 
				_areLocalVerticesDirty = true; 
			}
		} 
	}

	
	override public float anchorX 
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
	
	override public float anchorY 
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
	
	public float scrollX 
	{
		get { return _scrollX;}
		set 
		{ 
			if(_scrollX != value)
			{
				_scrollX = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public float scrollY 
	{
		get { return _scrollY;}
		set 
		{ 
			if(_scrollY != value)
			{
				_scrollY = value; 
				_isMeshDirty = true; 
			}
		}
	}

}
	
	

