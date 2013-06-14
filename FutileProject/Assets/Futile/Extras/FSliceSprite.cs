using UnityEngine;
using System;

public class FSliceSprite : FSprite
{
	private float _insetTop;
	private float _insetRight;
	private float _insetBottom;
	private float _insetLeft;
	
	private float _width;
	private float _height;
	
	private int _sliceCount;
	
	private Vector2[] _uvVertices;
	
	public FSliceSprite (string elementName, float width, float height, float insetTop, float insetRight, float insetBottom, float insetLeft) : this(Futile.atlasManager.GetElementWithName(elementName), width, height, insetTop, insetRight, insetBottom, insetLeft)
	{
	}
	
	public FSliceSprite (FAtlasElement element, float width, float height, float insetTop, float insetRight, float insetBottom, float insetLeft) : base()
	{
		_width = width;
		_height = height;
		
		_insetTop = insetTop;
		_insetRight = insetRight;
		_insetBottom = insetBottom;
		_insetLeft = insetLeft;
		
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
		_insetTop = Math.Max (0,_insetTop);
		_insetRight = Math.Max(0,_insetRight);
		_insetBottom = Math.Max (0,_insetBottom);
		_insetLeft = Math.Max(0,_insetLeft);
		
		_sliceCount = 1;
		
		if(_insetTop > 0) _sliceCount++;
		if(_insetRight > 0) _sliceCount++;
		if(_insetLeft > 0) _sliceCount++;
		if(_insetBottom > 0) _sliceCount++;
		
		if(_insetTop > 0 && _insetRight > 0) _sliceCount++;
		if(_insetTop > 0 && _insetLeft > 0) _sliceCount++;
		if(_insetBottom > 0 && _insetRight > 0) _sliceCount++;
		if(_insetBottom > 0 && _insetLeft > 0) _sliceCount++;
		
		_numberOfFacetsNeeded = _sliceCount;
		
		_localVertices = new Vector2[_sliceCount*4];
		_uvVertices = new Vector2[_sliceCount*4];
		
		_areLocalVerticesDirty = true;
		
		if(_numberOfFacetsNeeded != _sliceCount)
		{
			_numberOfFacetsNeeded = _sliceCount;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}

		UpdateLocalVertices();
	}
	
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		
		Rect uvRect = element.uvRect;
		
		float itop = Math.Max(0,Math.Min(_insetTop, _element.sourceSize.y-_insetBottom));
		float iright = Math.Max(0,Math.Min(_insetRight, _element.sourceSize.x-_insetLeft));
		float ibottom = Math.Max(0,Math.Min(_insetBottom, _element.sourceSize.y-_insetTop));
		float ileft = Math.Max(0,Math.Min(_insetLeft, _element.sourceSize.x-_insetRight));
		
		float uvtop = uvRect.height*(itop/_element.sourceSize.y);
		float uvleft = uvRect.width*(ileft/_element.sourceSize.x);
		float uvbottom = uvRect.height*(ibottom/_element.sourceSize.y);
		float uvright = uvRect.width*(iright/_element.sourceSize.x);
		
		_textureRect.x = -_anchorX*_width;
		_textureRect.y = -_anchorY*_height;
		_textureRect.width = _width;
		_textureRect.height = _height;
		
		_localRect = _textureRect;
		
		
		float localXMin = _localRect.xMin;
		float localXMax = _localRect.xMax;
		float localYMin = _localRect.yMin;
		float localYMax = _localRect.yMax;
		
		float uvXMin = uvRect.xMin;
		float uvXMax = uvRect.xMax;
		float uvYMin = uvRect.yMin;
		float uvYMax = uvRect.yMax;
		
		int sliceVertIndex = 0;
		
		for(int s = 0; s<9; s++)
		{
			if(s == 0) //center slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin + ileft,localYMax - itop);
				_localVertices[sliceVertIndex+1].Set (localXMax - iright,localYMax - itop);
				_localVertices[sliceVertIndex+2].Set (localXMax - iright,localYMin + ibottom);
				_localVertices[sliceVertIndex+3].Set (localXMin + ileft,localYMin + ibottom);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin + uvleft,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+1].Set (uvXMax - uvright,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+2].Set (uvXMax - uvright,uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+3].Set (uvXMin + uvleft,uvYMin + uvbottom);
				
				sliceVertIndex += 4;
			}
			else if (s == 1 && _insetTop > 0) //top center slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin + ileft,localYMax);
				_localVertices[sliceVertIndex+1].Set (localXMax - iright,localYMax);
				_localVertices[sliceVertIndex+2].Set (localXMax - iright,localYMax - itop);
				_localVertices[sliceVertIndex+3].Set (localXMin + ileft,localYMax - itop);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin + uvleft,uvYMax);
				_uvVertices[sliceVertIndex+1].Set (uvXMax - uvright,uvYMax);
				_uvVertices[sliceVertIndex+2].Set (uvXMax - uvright,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+3].Set (uvXMin + uvleft,uvYMax - uvtop);
				
				sliceVertIndex += 4;	
			}
			else if (s == 2 && _insetRight > 0) //right center slice
			{
				_localVertices[sliceVertIndex].Set   (localXMax - iright,localYMax - itop);
				_localVertices[sliceVertIndex+1].Set (localXMax,localYMax - itop);
				_localVertices[sliceVertIndex+2].Set (localXMax,localYMin + ibottom);
				_localVertices[sliceVertIndex+3].Set (localXMax - iright,localYMin + ibottom);
				
				_uvVertices[sliceVertIndex].Set   (uvXMax - uvright,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+1].Set (uvXMax,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+2].Set (uvXMax,uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+3].Set (uvXMax - uvright,uvYMin + uvbottom);
				
				sliceVertIndex += 4;	
			}
			else if (s == 3 && _insetBottom > 0) //bottom center slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin + ileft,localYMin + ibottom);
				_localVertices[sliceVertIndex+1].Set (localXMax - iright,localYMin + ibottom);
				_localVertices[sliceVertIndex+2].Set (localXMax - iright,localYMin);
				_localVertices[sliceVertIndex+3].Set (localXMin + ileft,localYMin);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin + uvleft,uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+1].Set (uvXMax - uvright,uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+2].Set (uvXMax - uvright,uvYMin);
				_uvVertices[sliceVertIndex+3].Set (uvXMin + uvleft,uvYMin);
				
				sliceVertIndex += 4;	
			}
			else if (s == 4 && _insetLeft > 0) //left center slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin,localYMax - itop);
				_localVertices[sliceVertIndex+1].Set (localXMin + ileft,localYMax - itop);
				_localVertices[sliceVertIndex+2].Set (localXMin + ileft,localYMin + ibottom);
				_localVertices[sliceVertIndex+3].Set (localXMin,localYMin + ibottom);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+1].Set (uvXMin + uvleft,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+2].Set (uvXMin + uvleft,uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+3].Set (uvXMin,uvYMin + uvbottom);
				
				sliceVertIndex += 4;	
			}
			else if (s == 5 && _insetTop > 0 && _insetLeft > 0) //top left slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin,localYMax);
				_localVertices[sliceVertIndex+1].Set (localXMin + ileft,localYMax);
				_localVertices[sliceVertIndex+2].Set (localXMin + ileft,localYMax - itop);
				_localVertices[sliceVertIndex+3].Set (localXMin,localYMax - itop);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin,uvYMax);
				_uvVertices[sliceVertIndex+1].Set (uvXMin + uvleft,uvYMax);
				_uvVertices[sliceVertIndex+2].Set (uvXMin + uvleft,uvYMax - uvtop);
				_uvVertices[sliceVertIndex+3].Set (uvXMin,uvYMax - uvtop);
				
				sliceVertIndex += 4;	
			}
			else if (s == 6 && _insetTop > 0 && _insetRight > 0) //top right slice
			{
				_localVertices[sliceVertIndex].Set   (localXMax - iright,localYMax);
				_localVertices[sliceVertIndex+1].Set (localXMax,localYMax);
				_localVertices[sliceVertIndex+2].Set (localXMax,localYMax - itop);
				_localVertices[sliceVertIndex+3].Set (localXMax - iright,localYMax - itop);
				
				_uvVertices[sliceVertIndex].Set   (uvXMax - uvright, uvYMax);
				_uvVertices[sliceVertIndex+1].Set (uvXMax, uvYMax);
				_uvVertices[sliceVertIndex+2].Set (uvXMax, uvYMax - uvtop);
				_uvVertices[sliceVertIndex+3].Set (uvXMax - uvright, uvYMax - uvtop);
				
				sliceVertIndex += 4;	
			}
			else if (s == 7 && _insetBottom > 0 && _insetRight > 0) //bottom right slice
			{
				_localVertices[sliceVertIndex].Set   (localXMax - iright,localYMin + ibottom);
				_localVertices[sliceVertIndex+1].Set (localXMax,localYMin + ibottom);
				_localVertices[sliceVertIndex+2].Set (localXMax,localYMin);
				_localVertices[sliceVertIndex+3].Set (localXMax - iright,localYMin);
				
				_uvVertices[sliceVertIndex].Set   (uvXMax - uvright, uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+1].Set (uvXMax, uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+2].Set (uvXMax, uvYMin);
				_uvVertices[sliceVertIndex+3].Set (uvXMax - uvright, uvYMin);
				
				sliceVertIndex += 4;	
			}
			else if (s == 8 && _insetBottom > 0 && _insetLeft > 0) //bottom left slice
			{
				_localVertices[sliceVertIndex].Set   (localXMin,localYMin + ibottom);
				_localVertices[sliceVertIndex+1].Set (localXMin + ileft,localYMin + ibottom);
				_localVertices[sliceVertIndex+2].Set (localXMin + ileft,localYMin);
				_localVertices[sliceVertIndex+3].Set (localXMin,localYMin);
				
				_uvVertices[sliceVertIndex].Set   (uvXMin, uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+1].Set (uvXMin + uvleft, uvYMin + uvbottom);
				_uvVertices[sliceVertIndex+2].Set (uvXMin + uvleft, uvYMin);
				_uvVertices[sliceVertIndex+3].Set (uvXMin, uvYMin);
				
				sliceVertIndex += 4;	
			}
		}
		
		_isMeshDirty = true;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			for(int s = 0; s<_sliceCount; s++)
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
				
				colors[vertexIndex0] = _alphaColor;
				colors[vertexIndex1] = _alphaColor;
				colors[vertexIndex2] = _alphaColor;
				colors[vertexIndex3] = _alphaColor;
				
				_renderLayer.HandleVertsChange();
			}
		}
	}
	
	public void SetInsets(float insetTop, float insetRight, float insetBottom, float insetLeft)
	{
		_insetTop = insetTop;
		_insetRight = insetRight;
		_insetBottom = insetBottom;
		_insetLeft = insetLeft;
		
		SetupSlices();
	}
	
	override public float width
	{
		get { return _width; }
		set { _width = value; _areLocalVerticesDirty = true; } 
	}
	
	override public float height
	{
		get { return _height; }
		set { _height = value; _areLocalVerticesDirty = true; } 
	}
}

