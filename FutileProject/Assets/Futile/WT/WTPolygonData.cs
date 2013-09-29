using UnityEngine;
using System;
using System.Collections.Generic;

public class WTPolygonData
{
	public bool shouldUseSmoothSphereCollisions = false; //set to true manually if needed

	private Vector2[] polygonVertices_;

	public int[] polygonTriangles {get; private set;} //a list of triangle polygons (each one is an array of int triangle indices)

	public WTPolygonData (Vector2[] vertices)
	{
		this.polygonVertices = vertices;
	}

	public Vector2[] polygonVertices { //VERTICES MUST BE PROVIDED IN CLOCKWISE ORDER!
		get {return polygonVertices_;}
		set {
			polygonVertices_ = value;
			polygonTriangles = FPUtils.Triangulate(polygonVertices_);
		}
	}
}