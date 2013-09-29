using UnityEngine;
using System;

/*
A sprite split in 2 parts, bottom, and top, each with its own color and alpha values.
*/

public class FSplitSprite : FSprite
{
	protected Color _bottomColor=Futile.white;
	protected Color _topColor=Futile.white;

	protected Color _bottomAlphaColor=Futile.white;
	protected Color _topAlphaColor=Futile.white;

	protected float _bottomAlpha=1.0f;
	protected float _topAlpha=1.0f;
	
	protected float _splitRatio=0.5f;
	
	protected Vector2[] _uvVertices;
	protected Color[] _uvColors;
	
	public FSplitSprite (string elementName) : this(Futile.atlasManager.GetElementWithName(elementName))
	{
	}
	
	public FSplitSprite (FAtlasElement element) : base()
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
		_numberOfFacetsNeeded = 2;
		
		_localVertices = new Vector2[_numberOfFacetsNeeded*4];
		_uvVertices = new Vector2[_numberOfFacetsNeeded*4];
		_uvColors = new Color[_numberOfFacetsNeeded];
		
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
		
		_localVertices[0].Set(left,bottom + sourceHeight*_splitRatio);
		_localVertices[1].Set(left + sourceWidth,bottom + sourceHeight*_splitRatio);
		_localVertices[2].Set(left + sourceWidth,bottom);
		_localVertices[3].Set(left,bottom);
		
		_localVertices[4].Set(left,bottom + sourceHeight);
		_localVertices[5].Set(left + sourceWidth,bottom + sourceHeight);
		_localVertices[6].Set(_localVertices[1].x,_localVertices[1].y);
		_localVertices[7].Set(_localVertices[0].x,_localVertices[0].y);
		
		_uvVertices[0] = new Vector2(_element.uvBottomLeft.x,_element.uvBottomRight.y+_element.uvRect.height*_splitRatio);
		_uvVertices[1] = new Vector2(_element.uvBottomRight.x,_element.uvBottomLeft.y+_element.uvRect.height*_splitRatio);
		_uvVertices[2] = _element.uvBottomRight;
		_uvVertices[3] = _element.uvBottomLeft;

		_uvVertices[4] = _element.uvTopLeft;
		_uvVertices[5] = _element.uvTopRight;
		_uvVertices[6] = _uvVertices[1];
		_uvVertices[7] = _uvVertices[0];

		_isMeshDirty = true;
	}

	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if (_isAlphaDirty|| shouldForceDirty) {
			_bottomColor.ApplyMultipliedAlpha(ref _bottomAlphaColor, _bottomAlpha*_concatenatedAlpha);
			_topColor.ApplyMultipliedAlpha(ref _topAlphaColor, _topAlpha*_concatenatedAlpha);
			_uvColors[0]=_bottomAlphaColor;
			_uvColors[1]=_topAlphaColor;
		}
		base.Redraw(shouldForceDirty,shouldUpdateDepth);
	}
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;

			for(int s = 0; s<_numberOfFacetsNeeded; s++)
			{
				int sliceVertIndex = s*4;
				int vertexIndex0 = (_firstFacetIndex+s)*4;
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
				
				uvs[vertexIndex0] = _uvVertices[sliceVertIndex];
				uvs[vertexIndex1] = _uvVertices[sliceVertIndex+1];
				uvs[vertexIndex2] = _uvVertices[sliceVertIndex+2];
				uvs[vertexIndex3] = _uvVertices[sliceVertIndex+3];
				
				colors[vertexIndex0] = _uvColors[s];
				colors[vertexIndex1] = _uvColors[s];
				colors[vertexIndex2] = _uvColors[s];
				colors[vertexIndex3] = _uvColors[s];
				
			}
			_renderLayer.HandleVertsChange();
		}
	}
	
	virtual public float splitRatio 
	{
		get { return _splitRatio; }
		set 
		{ 
			float newSplitRatio = Math.Max (0.0f, Math.Min (1.0f, value));
			if(_splitRatio != newSplitRatio)
			{
				_splitRatio = newSplitRatio; 
				_areLocalVerticesDirty = true;
			}
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


