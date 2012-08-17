using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FRenderLayer
{
	public int batchIndex;
	
	private FStage _stage;
	
	private FAtlas _atlas;
	private FShader _shader;
	
	private GameObject _gameObject;
	private Transform _transform;
	private Material _material;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Mesh _mesh;
	
	//Mesh stuff
	private Vector3[] _vertices = new Vector3[0];
	private int[] _triangles = new int[0];
	private Vector2[] _uvs = new Vector2[0];
	private Color[] _colors = new Color[0];
	
	private bool _isMeshDirty = false;
	private bool _didVertsChange = false;
	private bool _didUVsChange = false;
	private bool _didColorsChange = false;
	private bool _didVertCountChange = false;
	private bool _doesMeshNeedClear = false;
	private bool _shouldUpdateBounds = false;

	private int _expansionAmount;
	private int _maxEmptyQuads;
	private int _maxQuadCount = 0;
	
	private int _depth = -1;
	private int _nextAvailableQuadIndex;
	
	private int _lowestZeroIndex = 0;
	
	private bool _needsRecalculateBoundsIfTransformed = false;
	
	public FRenderLayer (FStage stage, FAtlas atlas, FShader shader)
	{
		_stage = stage;
		
		_atlas = atlas;
		_shader = shader;
		
		_expansionAmount = Futile.quadsPerLayerExpansion;
		_maxEmptyQuads = Futile.maxEmptyQuadsPerLayer;
		
		batchIndex = atlas.index*10000 + shader.index;
		
		_gameObject = new GameObject("FRenderLayer ("+_stage.name+")");
		_transform = _gameObject.transform;
		
		_transform.parent = Futile.instance.gameObject.transform;
		
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
		
		UpdateTransform();
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(_gameObject);
	}

	public void UpdateTransform()
	{
		_transform.position = _stage.transform.position;
		_transform.rotation = _stage.transform.rotation;
		_transform.localScale = _stage.transform.localScale;
		
		if(_needsRecalculateBoundsIfTransformed)
		{
			_needsRecalculateBoundsIfTransformed = false;
			_mesh.RecalculateBounds();
		}
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
			_gameObject.name = "FRenderLayer X ("+_stage.name+") (" + _atlas.name + " " + _shader.name+")";
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
		//if we have a ton of empty quads
		//shrink the quads
		if(_nextAvailableQuadIndex < _maxQuadCount-_maxEmptyQuads)
		{
			ShrinkMaxQuadLimit(Math.Max (0,(_maxQuadCount-_nextAvailableQuadIndex)-_expansionAmount));	
			ExpandMaxQuadLimit(1);
		}
		
		_lowestZeroIndex = Math.Max (_nextAvailableQuadIndex, Math.Min (_maxQuadCount,_lowestZeroIndex));
		
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
			_gameObject.name = "FRenderLayer "+_depth+" ("+_stage.name+") ["+_nextAvailableQuadIndex+"/"+_maxQuadCount+"] (" + _atlas.name + " " + _shader.name+")";
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
			//see http://docs.unity3d.com/Documentation/ScriptReference/Mesh.html
			if(_doesMeshNeedClear) _mesh.Clear(); 
			_mesh.vertices = _vertices;
			_mesh.triangles = _triangles;
			_mesh.uv = _uvs;
			
			//TODO: switch to using colors32 at some point for performance
			//see http://docs.unity3d.com/Documentation/ScriptReference/Mesh-colors32.html
			_mesh.colors = _colors;
			
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
				//Ok nevermind, I put it back in for now because if you scroll the stage, it's needed
				
				_needsRecalculateBoundsIfTransformed = true;
				
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
	
	private void ShrinkMaxQuadLimit(int deltaDecrease)
	{
		if(deltaDecrease <= 0) return;
		
		_maxQuadCount = Math.Max (Futile.startingQuadsPerLayer, _maxQuadCount-deltaDecrease);
	
		//resize the arrays so they can fit everything
		Array.Resize (ref _vertices,_maxQuadCount*4);
		Array.Resize (ref _uvs,_maxQuadCount*4);
		Array.Resize (ref _colors,_maxQuadCount*4);
		Array.Resize (ref _triangles,_maxQuadCount*6);

		_didVertCountChange = true;
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
		_doesMeshNeedClear = true; //we only need clear when shrinking the mesh size
	}
	
	private void ExpandMaxQuadLimit(int deltaIncrease)
	{
		if(deltaIncrease <= 0) return;
		
		int firstNewQuadIndex = _maxQuadCount;
		
		_maxQuadCount += deltaIncrease;
		
		//shrink the arrays
		Array.Resize (ref _vertices,_maxQuadCount*4);
		Array.Resize (ref _uvs,_maxQuadCount*4);
		Array.Resize (ref _colors,_maxQuadCount*4);
		Array.Resize (ref _triangles,_maxQuadCount*6);
		
		//fill the triangles with the correct values
		for(int i = firstNewQuadIndex; i<_maxQuadCount; ++i)
		{
			_triangles[i*6 + 0] = i * 4 + 0;	
			_triangles[i*6 + 1] = i * 4 + 1;
			_triangles[i*6 + 2] = i * 4 + 2;
			
			_triangles[i*6 + 3] = i * 4 + 0;	
			_triangles[i*6 + 4] = i * 4 + 2;
			_triangles[i*6 + 5] = i * 4 + 3;
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
		
				//this will set the render order correctly based on the depth
				_material.renderQueue = 3000+_depth;
				
				#if UNITY_EDITOR
					//some debug code so that layers are sorted by depth properly
					_gameObject.name = "FRenderLayer "+_depth+" ("+_stage.name+") ["+_nextAvailableQuadIndex+"/"+_maxQuadCount+"] (" + _atlas.name + " " + _shader.name+")";
				#endif
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


