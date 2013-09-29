using UnityEngine;
using System;

/*

FShadowSprite is a FSprite with a shadow.

TODO :
setters for _shadowOffsetX, _shadowOffsetY, _shadowColor and _shadowAlphaRatio

*/

public class FShadowSprite : FSprite
{
	protected float _shadowOffsetX,_shadowOffsetY;
	protected float _shadowAlphaRatio=0.5f;
	protected Color _shadowColor=new Color(0,0,0);

	protected Color _shadowAlphaColor;
	
	//private Vector2[] _uvVertices;
	
	public FShadowSprite (string elementName, float shadowOffsetX, float shadowOffsetY) : this(Futile.atlasManager.GetElementWithName(elementName), shadowOffsetX, shadowOffsetY)
	{
	}
	
	public FShadowSprite (FAtlasElement element, float shadowOffsetX, float shadowOffsetY) : base()
	{
		_shadowOffsetX = shadowOffsetX;
		_shadowOffsetY = shadowOffsetY;
		
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
		
		_localVertices[0].Set(left + _shadowOffsetX,bottom + sourceHeight + _shadowOffsetY);
		_localVertices[1].Set(left + sourceWidth + _shadowOffsetX,bottom + sourceHeight + _shadowOffsetY);
		_localVertices[2].Set(left + sourceWidth + _shadowOffsetX,bottom + _shadowOffsetY);
		_localVertices[3].Set(left + _shadowOffsetX,bottom + _shadowOffsetY);

		_localVertices[4].Set(left,bottom + sourceHeight);
		_localVertices[5].Set(left + sourceWidth,bottom + sourceHeight);
		_localVertices[6].Set(left + sourceWidth,bottom);
		_localVertices[7].Set(left,bottom);

		_isMeshDirty = true;
	}

	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if (_isAlphaDirty|| shouldForceDirty) {
			_shadowColor.ApplyMultipliedAlpha(ref _shadowAlphaColor, _concatenatedAlpha*_shadowAlphaRatio);
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
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				if( s==0) {
					//Debug.Log("_shadowAlphaColor="+_shadowAlphaColor);
					colors[vertexIndex0] = _shadowAlphaColor;
					colors[vertexIndex1] = _shadowAlphaColor;
					colors[vertexIndex2] = _shadowAlphaColor;
					colors[vertexIndex3] = _shadowAlphaColor;
				} else {
					//Debug.Log("_alphaColor="+_alphaColor);
					colors[vertexIndex0] = _alphaColor;
					colors[vertexIndex1] = _alphaColor;
					colors[vertexIndex2] = _alphaColor;
					colors[vertexIndex3] = _alphaColor;
				}
			}

			_renderLayer.HandleVertsChange();
		}
	}
}

