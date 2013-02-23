using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class FPMesh2DCollider : MonoBehaviour 
{
	private Vector2[] _sourceVertices;
	private int[] _sourceTriangles;
	private MeshCollider[] _colliders;
	private bool _shouldDecompose;
	
	public void Init (Vector2[] sourceVertices)
	{
		Init (sourceVertices, true);	
	}
	
	public void Init (Vector2[] sourceVertices, bool shouldDecompose)
	{
		_sourceVertices = sourceVertices;	
		_sourceTriangles = FPUtils.Triangulate(_sourceVertices); //used by the debug renderer
		_shouldDecompose = shouldDecompose;	
		
		List<Vector2> sourceVerticesList = new List<Vector2>(sourceVertices.Length);
		
		for(int s = 0; s<sourceVertices.Length; s++)
		{
			sourceVerticesList.Add(sourceVertices[s]);	
		}
		
		if(_shouldDecompose)
		{
			sourceVerticesList.Reverse(); //the algorithm needs them in reverse order
			
			List<List<Vector2>> polygons = FPDecomposer.Decompose(sourceVerticesList);
			
			_colliders = new MeshCollider[polygons.Count];
			
			for(int p = 0; p<polygons.Count; p++)
			{
				GameObject polygonGameObject;
				
				if(polygons.Count == 1)
				{
					polygonGameObject = gameObject;
				}
				else 
				{
					polygonGameObject = new GameObject("Decomposed Convex Polygon");
					polygonGameObject.transform.parent = gameObject.transform;
					polygonGameObject.transform.localPosition = Vector3.zero;
				}
				
				polygons[p].Reverse();//they will be provided in CCW order, so let's turn them into CW for our purposes
				_colliders[p] = CreatePolygonCollider(polygonGameObject, polygons[p].ToArray());
			}
		}
		else //do some stuff to make a single convex poly instead 
		{
			_colliders = new MeshCollider[1];
			_colliders[0] = CreatePolygonCollider(gameObject, _sourceVertices);
		}
	}
	
	private MeshCollider CreatePolygonCollider(GameObject polygonGameObject, Vector2[] polygonVertices)
	{
		MeshCollider collider = polygonGameObject.AddComponent<MeshCollider>();
		
		int polygonVertCount = polygonVertices.Length;
		Mesh mesh = new Mesh();
		Vector3[] fullVerts = new Vector3[polygonVertCount*2];
		
		int[] polygonTriangles = FPUtils.Triangulate(polygonVertices); //note that these are triangle indexes, three ints = one triangle
		int polygonTriangleCount = polygonTriangles.Length;
		int[] fullTriangles = new int[polygonTriangleCount*2 + polygonVertCount*6];
		
		for(int t = 0; t<polygonTriangleCount; t+=3) //notice that it increments by 3
		{
			//front face triangles
			fullTriangles[t] = polygonTriangles[t];
			fullTriangles[t+1] = polygonTriangles[t+1];
			fullTriangles[t+2] = polygonTriangles[t+2];
			
			//back face triangles
			int backIndex = polygonTriangleCount+t;
			fullTriangles[backIndex] = polygonVertCount+polygonTriangles[t];
			//notice how the +1 and +2 are switched to put the back face triangle vertices in the correct CW order
			fullTriangles[backIndex+2] = polygonVertCount+polygonTriangles[t+1]; 
			fullTriangles[backIndex+1] = polygonVertCount+polygonTriangles[t+2];
		}
		
		int doubleTriangleCount = polygonTriangleCount*2;
		
		for(int v = 0; v<polygonVertCount; v++)
		{
			Vector2 vertSource = polygonVertices[v];
			//make one vert at the front, then duplicate that vert and put it at the back (DEFAULT_Z_THICKNESS)
			Vector3 resultVert = fullVerts[v] = new Vector3(vertSource.x*FPhysics.POINTS_TO_METERS, vertSource.y*FPhysics.POINTS_TO_METERS,0);
			resultVert.z = FPhysics.DEFAULT_Z_THICKNESS;
			fullVerts[v + polygonVertCount] = resultVert;
			
			int sixV = v*6;
			
			fullTriangles[doubleTriangleCount+sixV] = v;
			fullTriangles[doubleTriangleCount+sixV+1] = v+polygonVertCount;
			fullTriangles[doubleTriangleCount+sixV+2] = (((v+1) % polygonVertCount) + polygonVertCount);
			
			fullTriangles[doubleTriangleCount+sixV+3] = v;
			fullTriangles[doubleTriangleCount+sixV+4] = (((v+1) % polygonVertCount) + polygonVertCount);
			fullTriangles[doubleTriangleCount+sixV+5] = (v+1) % polygonVertCount;
		}
		
		mesh.vertices = fullVerts;
		mesh.triangles = fullTriangles;
			
		collider.sharedMesh = mesh;
		
		if(_shouldDecompose)
		{
			collider.convex = true; //we're decomposing so we'll always have convex stuff
		}
		else 
		{
			collider.convex = FPUtils.CheckIfConvex(_sourceVertices);
		}
		
		collider.smoothSphereCollisions = false;	
		
		return collider;
	}
	
	public Vector2[] sourceVertices
	{
		get {return _sourceVertices;}	
	}
	
	public int[] sourceTriangles
	{
		get {return _sourceTriangles;}	
	}
	
	public MeshCollider[] colliders
	{
		get {return _colliders;}	
	}
	
	public void OnDestroy()
	{
		int colliderCount = _colliders.Length;
		for(int c = 0; c<colliderCount; c++)
		{
			UnityEngine.Object.Destroy(_colliders[c]);	
		}
		_colliders = null;
	}
}











public class FPDebugMesh2DColliderView : FFacetNode
{
	private FPMesh2DCollider _mesh2D;
	private int _triangleCount; 
	
	private Color _color = Futile.white;
	private Color _alphaColor = Futile.white;
	
	private bool _isMeshDirty = false;
	private bool _areLocalVerticesDirty = false;
	
	private Vector2 _uvTopLeft;
	private Vector2 _uvBottomLeft;
	private Vector2 _uvBottomRight;
	
	
	public FPDebugMesh2DColliderView (string elementName, FPMesh2DCollider mesh2D)
	{
		_mesh2D = mesh2D;
		_triangleCount = _mesh2D.sourceTriangles.Length/3;
		
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
		
		_triangleCount = _mesh2D.sourceTriangles.Length/3;
		
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
			
			int[] sourceTriangles = _mesh2D.sourceTriangles;
			Vector2[] sourceVertices = _mesh2D.sourceVertices;
			
			for(int t = 0; t<_triangleCount; t++)
			{
				int vertexIndex0 = (_firstFacetIndex+t)*3;
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int threeT = t*3;
				
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], sourceVertices[sourceTriangles[threeT]],0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], sourceVertices[sourceTriangles[threeT+1]],0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], sourceVertices[sourceTriangles[threeT+2]],0);
				
				uvs[vertexIndex0] = _uvBottomLeft;
				uvs[vertexIndex1] = _uvTopLeft;
				uvs[vertexIndex2] = _uvBottomRight;
				
				colors[vertexIndex0] = _alphaColor;
				colors[vertexIndex1] = _alphaColor;
				colors[vertexIndex2] = _alphaColor;
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

