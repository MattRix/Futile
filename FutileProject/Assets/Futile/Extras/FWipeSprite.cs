using UnityEngine;
using System;

public class FWipeSprite : FSprite
{
	protected float _wipeTopAmount = 1.0f;
	protected float _wipeRightAmount = 1.0f;
	protected float _wipeBottomAmount = 1.0f;
	protected float _wipeLeftAmount = 1.0f;
	
	public FWipeSprite (string elementName) : base()
	{
		Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(elementName),1);
		
		_isAlphaDirty = true; 
		
		UpdateLocalVertices(); 
	}
		
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			//these numbers are relative to the bottom left of the rect
			//they're used for figuring out the actual visible rectangle area during the wipe
			//you can think of them like a normalized visiblity rectangle
			float useRight = Mathf.Max(1.0f - wipeRightAmount, wipeLeftAmount);
			float useLeft = Mathf.Min(1.0f - wipeRightAmount, useRight);
			
			float useTop = Mathf.Max(1.0f - wipeTopAmount, wipeBottomAmount);
			float useBottom = Mathf.Min(1.0f - wipeTopAmount,useTop);
			
			int vertexIndex0 = _firstFacetIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			float localWidth = (_localVertices[1].x - _localVertices[0].x);
			float localHeight = (_localVertices[1].y - _localVertices[2].y);
			
			Vector2 localVector0 = new Vector2
			(
				_localVertices[0].x + localWidth * useLeft,
				_localVertices[3].y + localHeight * useTop 
			);
			
			Vector2 localVector1 = new Vector2
			(
				_localVertices[0].x + localWidth * useRight,
				_localVertices[3].y + localHeight * useTop 
			);
			
			Vector2 localVector2 = new Vector2
			(
				_localVertices[0].x + localWidth * useRight,
				_localVertices[3].y + localHeight * useBottom 
			);
			
			Vector2 localVector3 = new Vector2
			(
				_localVertices[0].x + localWidth * useLeft,
				_localVertices[3].y + localHeight * useBottom 
			);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], localVector0,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], localVector1,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], localVector2,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], localVector3,0);
			
			float uvWidth = (_element.uvTopRight.x - _element.uvTopLeft.x);
			float uvHeight = (_element.uvTopRight.y - _element.uvBottomRight.y);
			
			uvs[vertexIndex0] = new Vector2
			(
				_element.uvTopLeft.x + uvWidth * useLeft,
				_element.uvBottomLeft.y + uvHeight * useTop 
			);
			
			uvs[vertexIndex1] = new Vector2
			(
				_element.uvTopLeft.x + uvWidth * useRight,
				_element.uvBottomLeft.y + uvHeight * useTop 
			);
			
			uvs[vertexIndex2] = new Vector2
			(
				_element.uvTopLeft.x + uvWidth * useRight,
				_element.uvBottomLeft.y + uvHeight * useBottom 
			);
			
			uvs[vertexIndex3] = new Vector2
			(
				_element.uvTopLeft.x + uvWidth * useLeft,
				_element.uvBottomLeft.y + uvHeight * useBottom 
			);
			
			colors[vertexIndex0] = _alphaColor;
			colors[vertexIndex1] = _alphaColor;
			colors[vertexIndex2] = _alphaColor;
			colors[vertexIndex3] = _alphaColor;
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	public float wipeTopAmount 
	{
		get { return _wipeTopAmount;}
		set 
		{ 
			value = Mathf.Clamp01(value);
			if(_wipeTopAmount != value)
			{
				_wipeTopAmount = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public float wipeRightAmount 
	{
		get { return _wipeRightAmount;}
		set 
		{ 
			value = Mathf.Clamp01(value);
			if(_wipeRightAmount != value)
			{
				_wipeRightAmount = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public float wipeBottomAmount 
	{
		get { return _wipeBottomAmount;}
		set 
		{ 
			value = Mathf.Clamp01(value);
			if(_wipeBottomAmount != value)
			{
				_wipeBottomAmount = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public float wipeLeftAmount 
	{
		get { return _wipeLeftAmount;}
		set 
		{ 
			value = Mathf.Clamp01(value);
			if(_wipeLeftAmount != value)
			{
				_wipeLeftAmount = value; 
				_isMeshDirty = true; 
			}
		}
	}

}
	
	

