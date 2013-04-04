using UnityEngine;
using System;
using System.Collections.Generic;

public class FPPolygonalData
{
	public bool shouldDecomposeIntoConvexPolygons;
	
	public bool shouldUseSmoothSphereCollisions = false; //set to true manually if needed
	
	public Vector2[] sourceVertices; //VERTICES MUST BE PROVIDED IN CLOCKWISE ORDER!
	
	public List<Vector2[]> vertexPolygons; //a list of vertex polygons (each one is an array of Vector2 vertices)
	public List<int[]> trianglePolygons; //a list of triangle polygons (each one is an array of int triangle indices)
	
	public Mesh[] meshes; //meshes made from the polygons, for doing collisions
	
	public FPPolygonalData (Vector2[] vertices, bool shouldDecomposeIntoConvexPolygons)
	{
		this.sourceVertices = vertices;
		this.shouldDecomposeIntoConvexPolygons = shouldDecomposeIntoConvexPolygons;
		
		if(shouldDecomposeIntoConvexPolygons)
		{
			List<Vector2> sourceVerticesList = new List<Vector2>(sourceVertices);
			
			sourceVerticesList.Reverse(); //the algorithm needs them in reverse order
			
			vertexPolygons = FPDecomposer.Decompose(sourceVerticesList);
			int polygonCount = vertexPolygons.Count;
			meshes = new Mesh[polygonCount];
			trianglePolygons = new List<int[]>(polygonCount);
			
			for(int p = 0; p<polygonCount; p++)
			{
				Array.Reverse (vertexPolygons[p]);//they will be returned in CCW order, so let's turn them back into CW for our purposes
				
				trianglePolygons.Add (null); //it'll be written in CreateMeshFromPolygon
				
				meshes[p] = CreateMeshFromPolygon(p);
			}
		}
		else 
		{
			meshes = new Mesh[1];
			
			vertexPolygons = new List<Vector2[]>(1);
			vertexPolygons.Add(sourceVertices);
			
			trianglePolygons = new List<int[]>(1);
			trianglePolygons.Add (null); //it'll be written in CreateMeshFromPolygon
			meshes[0] = CreateMeshFromPolygon(0);
		}
	}
	
	private Mesh CreateMeshFromPolygon(int polygonIndex)
	{
		Vector2[] polygonVertices = vertexPolygons[polygonIndex]; 
		int[] polygonTriangles = trianglePolygons[polygonIndex];
		
		int polygonVertCount = polygonVertices.Length;
		
		if(polygonTriangles == null) //if we don't have any polygon triangles, create some!
		{
			polygonTriangles = trianglePolygons[polygonIndex] = FPUtils.Triangulate(polygonVertices); //note that these are triangle indexes, three ints = one triangle
		}
		
		int polygonTriangleCount = polygonTriangles.Length;
		
		Mesh mesh = new Mesh();
		Vector3[] meshVerts = new Vector3[polygonVertCount*2];
		int[] meshTriangles = new int[polygonTriangleCount*2 + polygonVertCount*6];
		
		for(int t = 0; t<polygonTriangleCount; t+=3) //notice that it increments by 3
		{
			//front face triangles
			meshTriangles[t] = polygonTriangles[t];
			meshTriangles[t+1] = polygonTriangles[t+1];
			meshTriangles[t+2] = polygonTriangles[t+2];
			
			//back face triangles
			int backIndex = polygonTriangleCount+t;
			meshTriangles[backIndex] = polygonVertCount+polygonTriangles[t];
			//notice how the +1 and +2 are switched to put the back face triangle vertices in the correct CW order
			meshTriangles[backIndex+2] = polygonVertCount+polygonTriangles[t+1]; 
			meshTriangles[backIndex+1] = polygonVertCount+polygonTriangles[t+2];
		}
		
		int doubleTriangleCount = polygonTriangleCount*2;
		
		for(int v = 0; v<polygonVertCount; v++)
		{
			Vector2 vertSource = polygonVertices[v];
			//make one vert at the front, then duplicate that vert and put it at the back (DEFAULT_Z_THICKNESS)
			Vector3 resultVert = meshVerts[v] = new Vector3(vertSource.x*FPhysics.POINTS_TO_METERS, vertSource.y*FPhysics.POINTS_TO_METERS,0);
			resultVert.z = FPhysics.DEFAULT_Z_THICKNESS;
			meshVerts[v + polygonVertCount] = resultVert;
			
			int sixV = v*6;
			
			meshTriangles[doubleTriangleCount+sixV] = v;
			meshTriangles[doubleTriangleCount+sixV+1] = v+polygonVertCount;
			meshTriangles[doubleTriangleCount+sixV+2] = (((v+1) % polygonVertCount) + polygonVertCount);
			
			meshTriangles[doubleTriangleCount+sixV+3] = v;
			meshTriangles[doubleTriangleCount+sixV+4] = (((v+1) % polygonVertCount) + polygonVertCount);
			meshTriangles[doubleTriangleCount+sixV+5] = (v+1) % polygonVertCount;
		}
		
		mesh.vertices = meshVerts;
		mesh.triangles = meshTriangles;	
		
		return mesh;
	}
	
	public FPPolygonalData (List<Vector2[]> vertexPolygons, List<int[]> trianglePolygons) //provide your own vertices and triangles
	{
		this.vertexPolygons = vertexPolygons;
		this.trianglePolygons = trianglePolygons;
		
		int polygonCount = vertexPolygons.Count;
		meshes = new Mesh[polygonCount];
		
		for(int p = 0; p<polygonCount; p++)
		{
			meshes[p] = CreateMeshFromPolygon(p);
		}
	}
	
}

