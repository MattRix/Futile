using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FRenderLayer
{
	public int batchIndex;
	
	private FAtlas _atlas;
	private FShader _shader;
	
	private GameObject _gameObject;
	private Material _material;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Mesh _mesh;
	
	//Mesh stuff
	private Vector3[] _vertices = new Vector3[0];
	private int[] _triIndices = new int[0];
	private Vector2[] _uvs = new Vector2[0];
	private Color[] _colors = new Color[0];
	
	private bool _isMeshDirty = false;
	private bool _didVertsChange = false;
	private bool _didUVsChange = false;
	private bool _didColorsChange = false;
	private bool _didVertCountChange = false;
	private bool _shouldUpdateBounds = false;

	private int _expansionAmount;
	private int _maxQuadCount = 0;
	
	private int _depth = -1;
	private int _nextAvailableQuadIndex;
	
	private int _lowestZeroIndex = 0;
	
	public FRenderLayer (FAtlas atlas, FShader shader)
	{
		_atlas = atlas;
		_shader = shader;
		
		_expansionAmount = Futile.quadsPerLayerExpansion;
		
		batchIndex = atlas.index*10000 + shader.index;
		
		_gameObject = new GameObject("FRenderLayer");
		_gameObject.transform.parent = Futile.instance.gameObject.transform;
		
		_meshFilter = _gameObject.AddComponent<MeshFilter>();
		_meshRenderer = _gameObject.AddComponent<MeshRenderer>();
		_meshRenderer.castShadows = false;
		_meshRenderer.receiveShadows = false;
		
		_mesh = _meshFilter.mesh;
		
		_material = new Material(_shader.shader);
		_material.mainTexture = _atlas.texture;
		
		_meshRenderer.renderer.material = _material;
		
		_gameObject.active = false;
		
		ExpandMaxQuadLimit(Futile.startingQuadsPerLayer);
	}
	
	public void AddToWorld () //add to the transform etc
	{
		_gameObject.active = true;
	}
	
	public void RemoveFromWorld() //remove it from the root transform etc
	{
		_gameObject.active = false;
		#if UNITY_EDITOR
			//some debug code so that layers are sorted by depth properly
			_gameObject.name = "FRenderLayer X (" + _atlas.atlasPath + " " + _shader.name+")";
		#endif
	}

	public void Open ()
	{
		_nextAvailableQuadIndex = 0;
	}
	
	public int GetNextQuadIndex (int numberOfQuadsNeeded)
	{		
		int indexToReturn = _nextAvailableQuadIndex;
		_nextAvailableQuadIndex += numberOfQuadsNeeded;
		
		//expand the layer (if needed) now that we know how many quads we need to fit
		if(_nextAvailableQuadIndex-1 >= _maxQuadCount)
		{
			int deltaNeeded = (_nextAvailableQuadIndex - _maxQuadCount) + 1;
			ExpandMaxQuadLimit(Math.Max (deltaNeeded, _expansionAmount)); //expand it by expansionAmount or the amount needed
		} 
			
		return indexToReturn;
	}

	public void Close () //fill remaining quads with 0,0,0
	{
		_lowestZeroIndex = Math.Max (_nextAvailableQuadIndex, _lowestZeroIndex);
		
		for(int z = _nextAvailableQuadIndex; z<_lowestZeroIndex; ++z)
		{
			int vertexIndex = z*4;	
			//the high 100000 Z should make them get culled and not rendered... 
			//TODO: test if the high z actually gives better performance or not
			_vertices[vertexIndex + 0].Set(0,0,100000);	
			_vertices[vertexIndex + 1].Set(0,0,100000);	
			_vertices[vertexIndex + 2].Set(0,0,100000);	
			_vertices[vertexIndex + 3].Set(0,0,100000);	
		}
		
		_lowestZeroIndex = _nextAvailableQuadIndex;
		
		#if UNITY_EDITOR
			//some debug code so that layers are sorted by depth properly
			_gameObject.name = "FRenderLayer "+_depth+" ["+_nextAvailableQuadIndex+"/"+_maxQuadCount+"] (" + _atlas.atlasPath + " " + _shader.name+")";
		#endif
		
	}
	
	//ACTUAL RENDERING GOES HERE
	
	public void Update() //called by the engine
	{
		if(_isMeshDirty)
		{
			UpdateMeshProperties();
		}
	}
	
	protected void UpdateMeshProperties()
	{
		_isMeshDirty = false;
		
		// Were changes made to the mesh since last time?
		
		if (_didVertCountChange) 
		{
			_didVertCountChange = false;
			_didColorsChange = false;
			_didVertsChange = false;
			_didUVsChange = false;
			_shouldUpdateBounds = false;
			
			//in theory we shouldn't need clear because we KNOW everything is correct
			//see http://docs.unity3d.com/Documentation/ScriptReference/Mesh.Clear.html
			//_mesh.Clear(); 
			
			_mesh.vertices = _vertices;
			_mesh.uv = _uvs;
			
			//TODO: switch to using color32 at some point for performance
			_mesh.colors = _colors;
			_mesh.triangles = _triIndices;
		}
		else 
		{
			if (_didVertsChange) 
			{
				_didVertsChange = false;
				_shouldUpdateBounds = true;
				
				_mesh.vertices = _vertices;
			}
		
			if (_shouldUpdateBounds) 
			{
				//Taking this out because it seems heavy, and I don't think there are benefits
				//http://docs.unity3d.com/Documentation/ScriptReference/Mesh.RecalculateBounds.html
				//_mesh.RecalculateBounds();
				
				_shouldUpdateBounds = false;
			}
		
			if (_didColorsChange) 
			{
				_didColorsChange = false;
				_mesh.colors = _colors;
			}
			
			if (_didUVsChange) 
			{
				_didUVsChange = false;
				_mesh.uv = _uvs;
			}
		} 
	}

	public void HandleVertsChange()
	{
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
	}
	
	private void ExpandMaxQuadLimit(int deltaIncrease)
	{
		int firstNewQuadIndex = _maxQuadCount;
		
		_maxQuadCount += deltaIncrease;
		
		// Vertices:
		Vector3[] tempVertices = _vertices;
		_vertices = new Vector3[_maxQuadCount * 4];
		tempVertices.CopyTo(_vertices, 0);

		// UVs:
		Vector2[] tempUVs = _uvs;
		_uvs = new Vector2[_maxQuadCount * 4];
		tempUVs.CopyTo(_uvs, 0);

		// Colors:
		Color[] tempColors = _colors;
		_colors = new Color[_maxQuadCount * 4];
		tempColors.CopyTo(_colors, 0);

		// Triangle indices:
		int[] tempTris = _triIndices;
		_triIndices = new int[_maxQuadCount * 6];
		tempTris.CopyTo(_triIndices, 0);
		
		for(int i = firstNewQuadIndex; i<_maxQuadCount; ++i)
		{
			_triIndices[i*6 + 0] = i * 4 + 0;	
			_triIndices[i*6 + 1] = i * 4 + 1;
			_triIndices[i*6 + 2] = i * 4 + 2;
			
			_triIndices[i*6 + 3] = i * 4 + 0;	
			_triIndices[i*6 + 4] = i * 4 + 2;
			_triIndices[i*6 + 5] = i * 4 + 3;
		}
		
		_didVertCountChange = true;
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
	}
	
	public int depth
	{
		get {return _depth;}
		set 
		{
			if(_depth != value)
			{
				_depth = value;
		
				_gameObject.transform.position = new Vector3(0,0,-_depth*0.0001f); //we multiply by a small number so it's subtle
			}
		}
	}
	
	public int expansionAmount
	{
		set {_expansionAmount = value;}
		get {return _expansionAmount;}
	}
	
	public Vector3[] vertices
	{
		get {return _vertices;}
		//set {_vertices = value;}
	}
	
	public Vector2[] uvs
	{
		get {return _uvs;}
		//set {_uvs = value;}
	}
	
	public Color[] colors
	{
		get {return _colors;}
		//set {_colors = value;}
	}
}


