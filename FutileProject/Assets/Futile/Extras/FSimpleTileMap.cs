using UnityEngine;
using System;
using System.Collections.Generic;

public class FSimpleTileMap : FMeshNode
{
	protected FAtlasElement[] _elements;

	protected int _cols;
	protected int _rows;

	protected float _tileWidth;
	protected float _tileHeight;
	
	protected float _anchorX = 0.5f;
	protected float _anchorY = 0.5f;

	//a simple tile map that takes an array of elements and renders it
	//note: the elements must all belong to the same atlas
	//note: elements.Length should equal cols*rows

	public FSimpleTileMap (FAtlasElement[] elements, int cols, int rows, float tileWidth, float tileHeight) : base()
	{
		_elements = elements;

		_cols = cols;
		_rows = rows;

		_tileWidth = tileWidth;
		_tileHeight = tileHeight;

		if(_elements.Length != _cols*_rows)
		{
			throw new FutileException("FSimpleTileMap - the number of elements does not match the number of rows and columns. It should be cols*rows = elements.Length");
		}
		
		Init(new FMeshData(FFacetType.Quad), _elements[0].atlas.fullElement);

		UpdateMesh();
	}

	public void UpdateMesh()
	{
		int tileCount = _elements.Length;

		_meshData.SetFacetCount(tileCount);

		float width = _cols*_tileWidth;
		float height = _rows*_tileHeight;

		float offsetX = -_anchorX*width;
		float offsetY = -_anchorY*height;

		List<FMeshFacet>quads = _meshData.facets;

		int i = 0;
		for(int r = 0; r<_rows; r++)
		{
			for(int c = 0; c<_cols; c++)
			{
				FMeshQuad quad = (FMeshQuad)quads[i];

				quad.SetUVRectFromElement(_elements[i]);
				quad.SetPosRect(offsetX+c*_tileWidth,offsetY+r*_tileHeight,_tileWidth,_tileHeight);

				i++;
			}
		}

		_meshData.MarkChanged();
	}

	public Vector2 GetTilePosition(int col, int row)
	{
		float offsetX = -_anchorX*width;
		float offsetY = -_anchorY*height;
		return new Vector2(offsetX + (col * _tileWidth) + _tileWidth/2, offsetY + (row*_tileHeight) + _tileHeight/2);
	}

	public FAtlasElement GetTileElement(int col, int row)
	{
		if(col < 0) return null;
		if(col >= _cols) return null;
		if(row < 0) return null;
		if(row >= _rows) return null;

		return _elements[row*_cols + row];
	}

	public FAtlasElement[] elements
	{
		set {_elements = elements; UpdateMesh();}
		get {return _elements;}
	}
	
	public float width
	{
		get { return _cols*_tileWidth; }
	}
	
	public float height
	{
		get { return _rows*_tileHeight;}
	}

	//TODO: allow user to change the elements

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
	
}



