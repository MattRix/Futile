using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FPPolygonCollider : MonoBehaviour 
{
	private FPPolygonData _polygonData;
	private MeshCollider[] _colliders;
	
	public void Init (FPPolygonData polygonData)
	{
		_polygonData = polygonData;
		
		int meshCount = _polygonData.meshes.Length;
		_colliders = new MeshCollider[meshCount];
		
		if(meshCount == 1)
		{
			_colliders[0] = CreatePolygonMeshCollider(gameObject, _polygonData.meshes[0]);
		}
		else 
		{
			for(int m = 0; m < meshCount; m++)
			{
				GameObject polygonGameObject = new GameObject("Decomposed Convex Polygon");
				polygonGameObject.transform.parent = gameObject.transform;
				polygonGameObject.transform.localPosition = Vector3.zero;
				
				_colliders[m] = CreatePolygonMeshCollider(polygonGameObject, _polygonData.meshes[m]);
			}
		}
	}
	
	private MeshCollider CreatePolygonMeshCollider(GameObject polygonGameObject, Mesh mesh)
	{
		MeshCollider collider = polygonGameObject.AddComponent<MeshCollider>();
		
		collider.sharedMesh = mesh;
		
		if(_polygonData.shouldDecomposeIntoConvexPolygons)
		{
			collider.convex = true; //we're decomposing so we'll always have convex stuff
		}
		else 
		{
			collider.convex = FPUtils.CheckIfConvex(_polygonData.sourceVertices);
		}
		
		collider.smoothSphereCollisions = _polygonData.shouldUseSmoothSphereCollisions;	
		
		return collider;
	}
	
	public FPPolygonData polygonData
	{
		get {return _polygonData;}	
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

public class FPDebugPolygonColliderView : FFacetNode
{
	private FPPolygonCollider _mesh2D;
	private int _triangleCount; 
	
	private Color _color = Futile.white;
	private Color _alphaColor = Futile.white;
	
	private bool _isMeshDirty = false;
	private bool _areLocalVerticesDirty = false;
	
	private Vector2 _uvTopLeft;
	private Vector2 _uvBottomLeft;
	private Vector2 _uvBottomRight;
	
	
	public FPDebugPolygonColliderView (string elementName, FPPolygonCollider mesh2D)
	{
		_mesh2D = mesh2D;
		
		List<int[]> trianglePolygons = _mesh2D.polygonData.trianglePolygons;
		
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
		
		List<int[]> trianglePolygons = _mesh2D.polygonData.trianglePolygons;
		
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
			
			List<Vector2[]> vertexPolygons = _mesh2D.polygonData.vertexPolygons;
			List<int[]> trianglePolygons = _mesh2D.polygonData.trianglePolygons;
		
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

public class FPPolygonData
{
	public bool shouldDecomposeIntoConvexPolygons;
	
	public bool shouldUseSmoothSphereCollisions = false; //set to true manually if needed
	
	public Vector2[] sourceVertices; //VERTICES MUST BE PROVIDED IN CLOCKWISE ORDER!
	
	public List<Vector2[]> vertexPolygons; //a list of vertex polygons (each one is an array of Vector2 vertices)
	public List<int[]> trianglePolygons; //a list of triangle polygons (each one is an array of int triangle indices)
	
	public Mesh[] meshes; //meshes made from the polygons, for doing collisions
	
	public FPPolygonData (Vector2[] vertices) : this(vertices,true) {}
	
	public FPPolygonData (Vector2[] vertices, bool shouldDecomposeIntoConvexPolygons)
	{
		this.sourceVertices = vertices;
		this.shouldDecomposeIntoConvexPolygons = shouldDecomposeIntoConvexPolygons;
		
		if(shouldDecomposeIntoConvexPolygons)
		{
			List<Vector2> sourceVerticesList = sourceVertices.ToList();
			
			sourceVerticesList.Reverse(); //the algorithm needs them in reverse order
			
			vertexPolygons = FPDecomposer.Decompose(sourceVerticesList);
			int polygonCount = vertexPolygons.Count;
			meshes = new Mesh[polygonCount];
			trianglePolygons = new List<int[]>(polygonCount);
			
			for(int p = 0; p<polygonCount; p++)
			{
				Array.Reverse (vertexPolygons[p]);//they will be returned in CCW order, so let's turn them back into CW for our purposes
				
				int[] triangles;
				meshes[p] = CreateMeshFromPolygon(vertexPolygons[p], out triangles);
				trianglePolygons.Add (triangles);
			}
		}
		else 
		{
			meshes = new Mesh[1];
			trianglePolygons = new List<int[]>(1);
			vertexPolygons = new List<Vector2[]>(1);
			vertexPolygons[0] = sourceVertices;
			int[] triangles;
			meshes[0] = CreateMeshFromPolygon(sourceVertices, out triangles);
			trianglePolygons.Add(triangles);
		}
		
		
		
	}
	
	private Mesh CreateMeshFromPolygon(Vector2[] polygonVertices, out int[] polygonTriangles)
	{
		int polygonVertCount = polygonVertices.Length;
		Mesh mesh = new Mesh();
		Vector3[] fullVerts = new Vector3[polygonVertCount*2];
		
		polygonTriangles = FPUtils.Triangulate(polygonVertices); //note that these are triangle indexes, three ints = one triangle
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
		
		return mesh;
	}
	
}

