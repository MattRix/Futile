using UnityEngine;
using System.Collections;

public static class WTUtils {
  // Line segment intersection based on this: http://www.pdas.com/lineint.html
	public static bool IntersectLineSegments(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
		Vector2 s1 = p1 - p0;
		Vector2 s2 = p3 - p2;

		float s, t, denom;

		denom = -s2.x * s1.y + s1.x * s2.y;

		if (denom == 0) {
			// divide by zero
			return false;
		}

		s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / denom;
		t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / denom;

		if (s >= 0 && s <= 1 && t >= 0 && t <= 1) return true;
		return false;
	}

  // Intersect circle/polygon algorithm based on this: http://gamedev.stackexchange.com/questions/7735/how-to-find-if-circle-and-concave-polygon-intersect
	public static bool IntersectCirclePolygon(Vector2 circlePos, float circleRadius, Vector2[] polygonVertices) {
		float xPolygonMin = float.MaxValue;
		float xPolygonMax = float.MinValue;
		float yPolygonMin = float.MaxValue;
		float yPolygonMax = float.MinValue;

		for (int i = 0; i < polygonVertices.Length; i++) {
			if (IntersectCircleLineSegment(circlePos, circleRadius, polygonVertices[i], polygonVertices[(i+1)%polygonVertices.Length])) return true;

			Vector2 v = polygonVertices[i];

			xPolygonMin = Mathf.Min(xPolygonMin, v.x);
			xPolygonMax = Mathf.Max(xPolygonMax, v.x);
			yPolygonMin = Mathf.Min(yPolygonMin, v.y);
			yPolygonMax = Mathf.Max(yPolygonMax, v.y);
		}

    // Expand the polygon just a bit to make sure everything is encompassed.
		float extra = 5;

		xPolygonMin += extra;
		xPolygonMax += extra;
		yPolygonMin += extra;
		yPolygonMax += extra;

		Vector2 circRayRight_p0 = circlePos;
		Vector2 circRayRight_p1 = new Vector2(10000, circlePos.y); // Extend the ray super far to the right

		float numIntersections = 0;

		for (int i = 0; i < polygonVertices.Length; i++) {
			Vector2 poly_p0 = polygonVertices[i];
			Vector2 poly_p1 = polygonVertices[(i+1)%polygonVertices.Length];

			if (poly_p0.y == poly_p1.y) continue;

			if (IntersectLineSegments(circRayRight_p0, circRayRight_p1, polygonVertices[i], polygonVertices[(i+1)%polygonVertices.Length])) numIntersections++;
		}

		if (numIntersections % 2 == 1) return true;
		else return false;
	}

	public static bool IntersectCircleLineSegment(Vector2 circlePos, float circleRadius, Vector2 segPointA, Vector2 segPointB) {
	    Vector2 upperPoint;
	    if (segPointA.y > segPointB.y) upperPoint = segPointA;
	    else upperPoint = segPointB;

	    Vector2 segVector = segPointB - segPointA;
		Vector2 aToCircVector = circlePos - segPointA;

		float closestSegPointMagnitude = Vector2.Dot(aToCircVector, segVector.normalized);

		Vector2 closestSegPoint;

		if (closestSegPointMagnitude < 0) closestSegPoint = segPointA;
		else if (closestSegPointMagnitude > segVector.magnitude) closestSegPoint = segPointB;
		else closestSegPoint = segPointA + closestSegPointMagnitude * segVector.normalized;

		Vector2 closestToCircVector = circlePos - closestSegPoint;

	    // Don't intersect with top point of line segment (keep it "open")
	    if (upperPoint.Equals(closestSegPoint)) {
	      if (closestToCircVector.magnitude < circleRadius) return true;
	    }
	    else {
	      if (closestToCircVector.magnitude <= circleRadius) return true;
	    }
			
	    return false;
	}
}