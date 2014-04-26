using UnityEngine;
using System;
using System.Collections.Generic;

public class FMeshNode : FFacetElementNode
{
	protected Color _color = Futile.white;
	protected Color _alphaColor = Futile.white;
	
	protected bool _isMeshDirty = false;

	protected FMeshData _meshData;
	protected int _previousMeshDataVersion;

	protected float _uvScaleX;
	protected float _uvScaleY;

	protected float _uvOffsetX;
	protected float _uvOffsetY;

	protected FMeshNode() : base() //for overriding
	{
	}

	public FMeshNode (FFacetType facetType, string elementName) : this(new FMeshData(facetType), Futile.atlasManager.GetElementWithName(elementName))
	{
	}

	public FMeshNode (FFacetType facetType, FAtlasElement element) : this(new FMeshData(facetType), element)
	{
	}
	
	public FMeshNode (FMeshData meshData, string elementName) : this(meshData, Futile.atlasManager.GetElementWithName(elementName))
	{
	}

	public FMeshNode (FMeshData meshData, FAtlasElement element) : base()
	{
		Init(meshData, element);
	}

	protected void Init(FMeshData meshData, FAtlasElement element)
	{
		_meshData = meshData;
		_previousMeshDataVersion = _meshData.version;
		
		Init(_meshData.facetType, element,meshData.facets.Count);

		_isMeshDirty = true;
		_isAlphaDirty = true;
	}

	public override void HandleAddedToStage()
	{
		base.HandleAddedToStage();

		if(_previousMeshDataVersion < _meshData.version) 
		{
			HandleMeshDataChanged();
		}

		_meshData.SignalUpdate += HandleMeshDataChanged;
	}

	public override void HandleRemovedFromStage()
	{
		base.HandleRemovedFromStage();

		_meshData.SignalUpdate -= HandleMeshDataChanged;
	}

	public void HandleMeshDataChanged()
	{
		_previousMeshDataVersion = _meshData.version;
		_isMeshDirty = true;

		int facetCount = _meshData.facets.Count;
		if(_numberOfFacetsNeeded != facetCount)
		{
			_numberOfFacetsNeeded = facetCount;
			_stage.HandleFacetsChanged();
		}
	}
	
	override public void HandleElementChanged()
	{
		_isMeshDirty = true;

		_uvScaleX = _element.uvRect.width;
		_uvScaleY = _element.uvRect.height;
		_uvOffsetX = _element.uvRect.xMin;
		_uvOffsetY = _element.uvRect.yMin;
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
		
		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;

			float a = _concatenatedMatrix.a;
			float b = _concatenatedMatrix.b;
			float c = _concatenatedMatrix.c;
			float d = _concatenatedMatrix.d;
			float tx = _concatenatedMatrix.tx;
			float ty = _concatenatedMatrix.ty;

			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			List<FMeshFacet> facets = _meshData.facets;
			int facetCount = facets.Count;

			FMeshVertex vertex;

			if(_meshData.facetType == FFacetType.Triangle)
			{
				int vertexIndex0 = _firstFacetIndex*3;
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;

				for(int f = 0; f<facetCount; f++)
				{
					FMeshFacet facet = facets[f];

					//outVector.x = localVector.x*a + localVector.y*c + tx;
					//outVector.y = localVector.x*b + localVector.y*d + ty;
					//outVector.z = z;

					FMeshVertex[] meshVertices = facet.vertices;

					vertex = meshVertices[0];
					vertices[vertexIndex0] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);
					vertex = meshVertices[1];
					vertices[vertexIndex1] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);
					vertex = meshVertices[2];
					vertices[vertexIndex2] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);

					//this needs to be offset by the element uvs, so that the uvs are relative to the element (add then multiply element uv)
					uvs[vertexIndex0] = new Vector2(_uvOffsetX + meshVertices[0].u * _uvScaleX,_uvOffsetY + meshVertices[0].v * _uvScaleY);
					uvs[vertexIndex1] = new Vector2(_uvOffsetX + meshVertices[1].u * _uvScaleX,_uvOffsetY + meshVertices[1].v * _uvScaleY);
					uvs[vertexIndex2] = new Vector2(_uvOffsetX + meshVertices[2].u * _uvScaleX,_uvOffsetY + meshVertices[2].v * _uvScaleY);

					//could also use vertex colours here!
					colors[vertexIndex0] = _alphaColor * meshVertices[0].color;
					colors[vertexIndex1] = _alphaColor * meshVertices[1].color;
					colors[vertexIndex2] = _alphaColor * meshVertices[2].color;

					vertexIndex0 += 3;
					vertexIndex1 += 3;
					vertexIndex2 += 3;
				}

			}
			else if(_meshData.facetType == FFacetType.Quad)
			{
				int vertexIndex0 = _firstFacetIndex*4;
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				for(int f = 0; f<facetCount; f++)
				{
					FMeshFacet facet = facets[f];
					
					FMeshVertex[] meshVertices = facet.vertices;
					vertex = meshVertices[0];
					vertices[vertexIndex0] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);
					vertex = meshVertices[1];
					vertices[vertexIndex1] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);
					vertex = meshVertices[2];
					vertices[vertexIndex2] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);
					vertex = meshVertices[3];
					vertices[vertexIndex3] = new Vector3(vertex.x*a + vertex.y*c + tx,vertex.x*b + vertex.y*d + ty,_meshZ);

					//this needs to be offset by the element uvs, so that the uvs are relative to the element (add then multiply element uv)
					uvs[vertexIndex0] = new Vector2(_uvOffsetX + meshVertices[0].u * _uvScaleX,_uvOffsetY + meshVertices[0].v * _uvScaleY);
					uvs[vertexIndex1] = new Vector2(_uvOffsetX + meshVertices[1].u * _uvScaleX,_uvOffsetY + meshVertices[1].v * _uvScaleY);
					uvs[vertexIndex2] = new Vector2(_uvOffsetX + meshVertices[2].u * _uvScaleX,_uvOffsetY + meshVertices[2].v * _uvScaleY);
					uvs[vertexIndex3] = new Vector2(_uvOffsetX + meshVertices[3].u * _uvScaleX,_uvOffsetY + meshVertices[3].v * _uvScaleY);
					
					//could also use vertex colours here!
					colors[vertexIndex0] = _alphaColor * meshVertices[0].color;
					colors[vertexIndex1] = _alphaColor * meshVertices[1].color;
					colors[vertexIndex2] = _alphaColor * meshVertices[2].color;
					colors[vertexIndex3] = _alphaColor * meshVertices[3].color;
					
					vertexIndex0 += 4;
					vertexIndex1 += 4;
					vertexIndex2 += 4;
					vertexIndex3 += 4;
				}
				
			}

			_renderLayer.HandleVertsChange();
		}
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

	public FMeshData meshData
	{
		get {return _meshData;}
		set 
		{
			if(_meshData != value)
			{
				_meshData = value;
				_previousMeshDataVersion = _meshData.version;
				_numberOfFacetsNeeded = _meshData.facets.Count;
				_isMeshDirty = true;
			}
		}
	}
}

