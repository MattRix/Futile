using UnityEngine;
using System;

/*

FBicolorSprite is a FSprite with 2 colors (bottom and top).

Usage :

FBicolorSprite bicolorSprite=new FBicolorSprite("Futile_White");
bicolorSprite.bottomColor=new Color(1f,0f,0f);
bicolorSprite.bottomAlpha=0.5f;
bicolorSprite.topColor=new Color(1f,0f,1f);
bicolorSprite.topAlpha=1.0f;
AddChild(bicolorSprite);

*/

public class FBicolorSprite : FSprite
{
	protected Color _bottomColor=Futile.white;
	protected Color _topColor=Futile.white;

	protected Color _bottomAlphaColor=Futile.white;
	protected Color _topAlphaColor=Futile.white;

	protected float _bottomAlpha=1.0f;
	protected float _topAlpha=1.0f;

	//private Vector2[] _uvVertices;
	
	public FBicolorSprite (string elementName) : this(Futile.atlasManager.GetElementWithName(elementName))
	{
	}
	
	public FBicolorSprite (FAtlasElement element) : base()
	{
		Init(FFacetType.Quad, element,0); //this will call HandleElementChanged(), which will call SetupSlices();
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	override public void HandleElementChanged()
	{
		SetupSlices();
	}

	public void SetupSlices ()
	{
		_numberOfFacetsNeeded = 1;
		
		_localVertices = new Vector2[_numberOfFacetsNeeded*4];
		//_uvVertices = new Vector2[_numberOfFacetsNeeded*4];
		
		_areLocalVerticesDirty = true;

		UpdateLocalVertices();
	}
	
	override public void UpdateLocalVertices()
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

	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if (_isAlphaDirty|| shouldForceDirty) {
			_bottomColor.ApplyMultipliedAlpha(ref _bottomAlphaColor, _bottomAlpha*_concatenatedAlpha);
			_topColor.ApplyMultipliedAlpha(ref _topAlphaColor, _topAlpha*_concatenatedAlpha);
		}
		base.Redraw(shouldForceDirty,shouldUpdateDepth);
	}
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;

			int sliceVertIndex = 0;
			int vertexIndex0 = (_firstFacetIndex+0)*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _localVertices[sliceVertIndex],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], _localVertices[sliceVertIndex+1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], _localVertices[sliceVertIndex+2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], _localVertices[sliceVertIndex+3],0);
			
			uvs[vertexIndex0] = _element.uvTopLeft;
			uvs[vertexIndex1] = _element.uvTopRight;
			uvs[vertexIndex2] = _element.uvBottomRight;
			uvs[vertexIndex3] = _element.uvBottomLeft;
			
			colors[vertexIndex0] = _topAlphaColor;
			colors[vertexIndex1] = _topAlphaColor;
			colors[vertexIndex2] = _bottomAlphaColor;
			colors[vertexIndex3] = _bottomAlphaColor;

			_renderLayer.HandleVertsChange();
		}
	}

	virtual public float bottomAlpha 
	{
		get { return _bottomAlpha; }
		set 
		{ 
			float newAlpha = Math.Max (0.0f, Math.Min (1.0f, value));
			if(_bottomAlpha != newAlpha)
			{
				_bottomAlpha = newAlpha; 
				_isAlphaDirty = true;
			}
		}
	}

	virtual public float topAlpha 
	{
		get { return _topAlpha; }
		set 
		{ 
			float newAlpha = Math.Max (0.0f, Math.Min (1.0f, value));
			if(_topAlpha != newAlpha)
			{
				_topAlpha = newAlpha; 
				_isAlphaDirty = true;
			}
		}
	}

	virtual public Color bottomColor 
	{
		get { return _bottomColor; }
		set 
		{ 
			if(_bottomColor != value)
			{
				_bottomColor = value; 
				_isAlphaDirty = true;
			}
		}
	}

	virtual public Color topColor 
	{
		get { return _topColor; }
		set 
		{ 
			if(_topColor != value)
			{
				_topColor = value; 
				_isAlphaDirty = true;
			}
		}
	}
}

