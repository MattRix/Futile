using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FPPolygonalCollider : MonoBehaviour 
{
	private FPPolygonalData _polygonalData;
	private MeshCollider[] _colliders;
	
	public void Init (FPPolygonalData polygonalData)
	{
		_polygonalData = polygonalData;
		
		int meshCount = _polygonalData.meshes.Length;
		_colliders = new MeshCollider[meshCount];
		
		if(meshCount == 1)
		{
			_colliders[0] = CreatePolygonMeshCollider(gameObject, _polygonalData.meshes[0]);
		}
		else 
		{
			for(int m = 0; m < meshCount; m++)
			{
				GameObject polygonGameObject = new GameObject("Decomposed Convex Polygon");
				polygonGameObject.transform.parent = gameObject.transform;
				polygonGameObject.transform.localPosition = Vector3.zero;
				
				_colliders[m] = CreatePolygonMeshCollider(polygonGameObject, _polygonalData.meshes[m]);
			}
		}
	}
	
	private MeshCollider CreatePolygonMeshCollider(GameObject polygonGameObject, Mesh mesh)
	{
		MeshCollider collider = polygonGameObject.AddComponent<MeshCollider>();
		
		collider.sharedMesh = mesh;
		
		if(_polygonalData.shouldDecomposeIntoConvexPolygons)
		{
			collider.convex = true; //we're decomposing so we'll always have convex stuff
		}
		else 
		{
			collider.convex = FPUtils.CheckIfConvex(_polygonalData.sourceVertices);
		}
		
		collider.smoothSphereCollisions = _polygonalData.shouldUseSmoothSphereCollisions;	
		
		return collider;
	}
	
	public FPPolygonalData polygonalData
	{
		get {return _polygonalData;}	
	}
	
	public MeshCollider[] colliders
	{
		get {return _colliders;}	
	}
	
	public void OnDestroy()
	{
//		int colliderCount = _colliders.Length;
//		for(int c = 0; c<colliderCount; c++)
//		{
//			UnityEngine.Object.Destroy(_colliders[c]);	
//		}
//		_colliders = null;
	}
}

public class FPDebugPolygonColliderView : FFacetElementNode
{
	private FPPolygonalCollider _mesh2D;
	private int _triangleCount; 
	
	private Color _color = Futile.white;
	private Color _alphaColor = Futile.white;
	
	private bool _isMeshDirty = false;
	private bool _areLocalVerticesDirty = false;
	
	private Vector2 _uvTopLeft;
	private Vector2 _uvBottomLeft;
	private Vector2 _uvBottomRight;
	
	
	public FPDebugPolygonColliderView (string elementName, FPPolygonalCollider mesh2D)
	{
		_mesh2D = mesh2D;
		
		List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		
		int polyCount = trianglePolygons.Count;
		
		_triangleCount = 0;
		
		for(int p = 0; p<polyCount; p++)
		{
			_triangleCount += trianglePolygons[p].Length / 3;	
		}
		
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName),_triangleCount);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
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
		
		if(_areLocalVerticesDirty)
		{
			UpdateLocalVertices();
		}
		
		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}
	
	override public void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
	}
	
	virtual public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		
		_uvTopLeft = _element.uvTopLeft;
		_uvBottomLeft = _element.uvBottomLeft;
		_uvBottomRight = _element.uvBottomRight;
		
		List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		
		int polyCount = trianglePolygons.Count;
		
		_triangleCount = 0;
		
		for(int p = 0; p<polyCount; p++)
		{
			_triangleCount += trianglePolygons[p].Length / 3;	
		}
		
		if(_numberOfFacetsNeeded != _triangleCount)
		{
			_numberOfFacetsNeeded = _triangleCount;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}
		
		_isMeshDirty = true;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			List<Vector2[]> vertexPolygons = _mesh2D.polygonalData.vertexPolygons;
			List<int[]> trianglePolygons = _mesh2D.polygonalData.trianglePolygons;
		
			int polyCount = trianglePolygons.Count;
			
			int nextTriangleIndex = _firstFacetIndex;
			
			for(int p = 0; p<polyCount; p++)
			{
				Vector2[] polyVertices = vertexPolygons[p];
				int[] polyTriangleIndices = trianglePolygons[p];
				
				int polyTriangleCount = polyTriangleIndices.Length /3;
				
				Color drawColor = RXColor.ColorFromHSL(0.8f+RXRandom.Float(p) * 0.3f,1f,0.5f);
				
				for(int t = 0; t < polyTriangleCount; t++)
				{
					int vertexIndex0 = nextTriangleIndex*3;
					int vertexIndex1 = vertexIndex0 + 1;
					int vertexIndex2 = vertexIndex0 + 2;
					int threeT = t*3;
					
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], polyVertices[polyTriangleIndices[threeT]],0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], polyVertices[polyTriangleIndices[threeT+1]],0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], polyVertices[polyTriangleIndices[threeT+2]],0);
					
					uvs[vertexIndex0] = _uvBottomLeft;
					uvs[vertexIndex1] = _uvTopLeft;
					uvs[vertexIndex2] = _uvBottomRight;
					
					colors[vertexIndex0] = drawColor;
					colors[vertexIndex1] = drawColor;
					colors[vertexIndex2] = drawColor;
					
//					colors[vertexIndex0] = _alphaColor;
//					colors[vertexIndex1] = _alphaColor;
//					colors[vertexIndex2] = _alphaColor;
					
					nextTriangleIndex++;
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
}

