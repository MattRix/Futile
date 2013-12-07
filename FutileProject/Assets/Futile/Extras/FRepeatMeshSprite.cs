using UnityEngine;
using System;
using System.Collections.Generic;

public class FRepeatMeshSprite : FMeshNode
{
	protected float _width;
	protected float _height;
	
	protected float _scrollX;
	protected float _scrollY;
	
	protected float _textureWidth;
	protected float _textureHeight;

	protected float _anchorX = 0.5f;
	protected float _anchorY = 0.5f;

	//FRepeatMesh creates an actual grid of tile geometry
	//This will look the same as FRepeatSprite,
	//except that it will work with any FAtlasElement, not just single images
	//You can set scrollX and scrollY to make the texture scroll
	
	public FRepeatMeshSprite (string elementName, float width, float height) : this(elementName,width,height,0,0) {}
	
	public FRepeatMeshSprite (string elementName, float width, float height, float scrollX, float scrollY) : base()
	{
		_width = width;
		_height = height;
		
		_scrollX = scrollX;
		_scrollY = scrollY;

		Init(new FMeshData(FFacetType.Quad), Futile.atlasManager.GetElementWithName(elementName));
	}

	public void UpdateMesh()
	{
		float offsetX = -_anchorX*width;
		float offsetY = -_anchorY*height;
		
		float ewidth = _element.sourceSize.x;
		float eheight = _element.sourceSize.y;
		
		float leftRemaining = RXMath.Mod(_scrollX, ewidth);
		float bottomRemaining = RXMath.Mod(_scrollY, eheight);
		
		float leftX = leftRemaining; 
		float rightX = leftX+width;
		
		int leftCol = 0;
		int rightCol = Mathf.FloorToInt(rightX / ewidth);
		if(rightX % ewidth == 0) rightCol--; //if it fits exactly, we don't need the last column
		
		
		float bottomY = bottomRemaining; 
		float topY = bottomY+height;
		
		int bottomRow = 0; 
		int topRow = Mathf.FloorToInt(topY / eheight);
		if(topY % eheight == 0) topRow--; //if it fits exactly, we don't need the last row
		
		int numCols = (rightCol-leftCol)+1;
		int numRows = (topRow-bottomRow)+1;
		
		_meshData.SetFacetCount(numCols*numRows);
		
		List<FMeshFacet>quads = _meshData.facets;
		
		float rightRemaining = ((rightCol+1)*ewidth - rightX);
		float topRemaining = ((topRow+1)*eheight - topY);
		
		int q = 0;
		
		for(int r = 0; r<numRows; r++)
		{
			float quadBottom = offsetY+r*eheight - bottomRemaining;
			float quadTop = quadBottom+eheight;
			float uvBottom = 0;
			float uvTop = 1.0f;
			
			if(r == 0)//bottomRow
			{
				quadBottom += bottomRemaining;
				uvBottom += bottomRemaining/eheight;
			}
			
			if(r == numRows-1)//topRow
			{
				quadTop -= topRemaining;
				uvTop -= topRemaining/eheight;
			}
			
			for(int c = 0; c<numCols; c++)
			{
				FMeshQuad quad = (FMeshQuad)quads[q];
				q++;
				
				float quadLeft = offsetX+c*ewidth - leftRemaining;
				float quadRight = quadLeft+ewidth;
				float uvLeft = 0;
				float uvRight = 1.0f;
				
				if(c == 0)//leftCol
				{
					quadLeft += leftRemaining;
					uvLeft += leftRemaining/ewidth;
				}
				
				if(c == numCols-1)//rightCol
				{
					quadRight -= rightRemaining;
					uvRight -= rightRemaining/ewidth;
				}
				
				quad.SetPosExtents(quadLeft,quadRight,quadBottom,quadTop);
				quad.SetUVExtents(uvLeft,uvRight,uvBottom,uvTop);
			}
		}
		
		_meshData.MarkChanged();
	}

	override public void HandleElementChanged()
	{
		base.HandleElementChanged();
		UpdateMesh();
	}

	public void SetSize(float width, float height)
	{
		_width = Mathf.Abs(width);
		_height = Mathf.Abs(height);
		UpdateMesh();
	}
	
	public float width
	{
		get { return _width; }
		set 
		{ 
			value = Mathf.Abs(value);
			if(_width != value)
			{
				_width = value; 
				UpdateMesh();
			}
		} 
	}
	
	public float height
	{
		get { return _height;}
		set 
		{ 
			value = Mathf.Abs(value);
			if(_height != value)
			{
				_height = value; 
				UpdateMesh(); 
			}
		} 
	}

	public void SetAnchor(float anchorX, float anchorY)
	{
		_anchorX = anchorX;
		_anchorY = anchorY;
		UpdateMesh();
	}
	
	public float anchorX 
	{
		get { return _anchorX;}
		set 
		{ 
			if(_anchorX != value)
			{
				_anchorX = value; 
				UpdateMesh();
			}
		}
	}
	
	public float anchorY 
	{
		get { return _anchorY;}
		set 
		{ 
			if(_anchorY != value)
			{
				_anchorY = value; 
				UpdateMesh();
			}
		}
	}

	public void SetScroll(float scrollX, float scrollY)
	{
		_scrollX = scrollX;
		_scrollY = scrollY;
		UpdateMesh();
	}
	
	public float scrollX 
	{
		get { return _scrollX;}
		set 
		{ 
			if(_scrollX != value)
			{
				_scrollX = value; 
				UpdateMesh();
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
				UpdateMesh();
			}
		}
	}
	
}



