using System.Collections.Generic;
using UnityEngine;

// This was adapted from Farseer Physics
// It turns concave polygons into convex polygons
// Vertex order MUST be counter-clockwise (so makes sure to reverse futile's CW order before passing them to this)
// No holes or overlapping lines allowed

//TODO: Convert all Vector2 lists into arrays for performance

/// Convex decomposition algorithm created by Mark Bayazit (http://mnbayazit.com/)
/// For more information about this algorithm, see http://mnbayazit.com/406/bayazit

public static class FPDecomposer
{
	public static int MAX_POLYGON_VERTICES = 50;
	
    public static List<Vector2[]> Decompose(List<Vector2> vertices)
    {
        List<Vector2[]> list = new List<Vector2[]>(MAX_POLYGON_VERTICES);
        float d, lowerDist, upperDist;
        Vector2 p;
        Vector2 lowerInt = new Vector2();
        Vector2 upperInt = new Vector2(); // intersection points
        int lowerIndex = 0, upperIndex = 0;
        List<Vector2> lowerPoly, upperPoly;

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (Reflex(i, vertices))
            {
                lowerDist = upperDist = float.MaxValue; // std::numeric_limits<qreal>::max();
                for (int j = 0; j < vertices.Count; ++j)
                {
                    // if line intersects with an edge
                    if (Left(At(i - 1, vertices), At(i, vertices), At(j, vertices)) &&
                        RightOn(At(i - 1, vertices), At(i, vertices), At(j - 1, vertices)))
                    {
                        // find the point of intersection
                        p = LineTools_LineIntersect(At(i - 1, vertices), At(i, vertices), At(j, vertices),
                                                    At(j - 1, vertices));
                        if (Right(At(i + 1, vertices), At(i, vertices), p))
                        {
                            // make sure it's inside the poly
                            d = SquareDist(At(i, vertices), p);
                            if (d < lowerDist)
                            {
                                // keep only the closest intersection
                                lowerDist = d;
                                lowerInt = p;
                                lowerIndex = j;
                            }
                        }
                    }

                    if (Left(At(i + 1, vertices), At(i, vertices), At(j + 1, vertices)) &&
                        RightOn(At(i + 1, vertices), At(i, vertices), At(j, vertices)))
                    {
                        p = LineTools_LineIntersect(At(i + 1, vertices), At(i, vertices), At(j, vertices),
                                                    At(j + 1, vertices));
                        if (Left(At(i - 1, vertices), At(i, vertices), p))
                        {
                            d = SquareDist(At(i, vertices), p);
                            if (d < upperDist)
                            {
                                upperDist = d;
                                upperIndex = j;
                                upperInt = p;
                            }
                        }
                    }
                }

                // if there are no vertices to connect to, choose a point in the middle
                if (lowerIndex == (upperIndex + 1) % vertices.Count)
                {
                    Vector2 sp = ((lowerInt + upperInt) / 2);

                    lowerPoly = Copy(i, upperIndex, vertices);
                    lowerPoly.Add(sp);
                    upperPoly = Copy(lowerIndex, i, vertices);
                    upperPoly.Add(sp);
                }
                else
                {
                    double highestScore = 0, bestIndex = lowerIndex;
                    while (upperIndex < lowerIndex) upperIndex += vertices.Count;
                    for (int j = lowerIndex; j <= upperIndex; ++j)
                    {
                        if (CanSee(i, j, vertices))
                        {
                            double score = 1 / (SquareDist(At(i, vertices), At(j, vertices)) + 1);
                            if (Reflex(j, vertices))
                            {
                                if (RightOn(At(j - 1, vertices), At(j, vertices), At(i, vertices)) &&
                                    LeftOn(At(j + 1, vertices), At(j, vertices), At(i, vertices)))
                                {
                                    score += 3;
                                }
                                else
                                {
                                    score += 2;
                                }
                            }
                            else
                            {
                                score += 1;
                            }
                            if (score > highestScore)
                            {
                                bestIndex = j;
                                highestScore = score;
                            }
                        }
                    }
                    lowerPoly = Copy(i, (int)bestIndex, vertices);
                    upperPoly = Copy((int)bestIndex, i, vertices);
                }
                list.AddRange(Decompose(lowerPoly));
                list.AddRange(Decompose(upperPoly));
                return list;
            }
        }

        // polygon is already convex
        if (vertices.Count > MAX_POLYGON_VERTICES)
        {
            lowerPoly = Copy(0, vertices.Count / 2, vertices);
            upperPoly = Copy(vertices.Count / 2, 0, vertices);
            list.AddRange(Decompose(lowerPoly));
            list.AddRange(Decompose(upperPoly));
        }
        else
            list.Add(vertices.ToArray());

        //The polygons are not guaranteed to be without collinear points. We remove
        //them to be sure.
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = CollinearSimplify(list[i], 0);
        }

        //Remove empty vertice collections
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Length == 0)
                list.RemoveAt(i);
        }

        return list;
    }
	
	private static Vector2 At(int i, List<Vector2> vertices)
    {
        int s = vertices.Count;
		return vertices[(i + s*10000000) % s];
    }

    private static List<Vector2> Copy(int i, int j, List<Vector2> vertices)
    {
        List<Vector2> p = new List<Vector2>(MAX_POLYGON_VERTICES);
        while (j < i) j += vertices.Count;
        //p.reserve(j - i + 1);
        for (; i <= j; ++i)
        {
            p.Add(At(i, vertices));
        }
        return p;
    }

    private static bool CanSee(int i, int j, List<Vector2> vertices)
    {
        if (Reflex(i, vertices))
        {
            if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) &&
                RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices))) return false;
        }
        else
        {
            if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) ||
                LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices))) return false;
        }
        if (Reflex(j, vertices))
        {
            if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) &&
                RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices))) return false;
        }
        else
        {
            if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) ||
                LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices))) return false;
        }
        for (int k = 0; k < vertices.Count; ++k)
        {
            if ((k + 1) % vertices.Count == i || k == i || (k + 1) % vertices.Count == j || k == j)
            {
                continue; // ignore incident edges
            }
            Vector2 intersectionPoint;
            if (LineTools_LineIntersect(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices), out intersectionPoint))
            {
                return false;
            }
        }
        return true;
    }

    // precondition: ccw
    private static bool Reflex(int i, List<Vector2> vertices)
    {
        return Right(i, vertices);
    }

    private static bool Right(int i, List<Vector2> vertices)
    {
        return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
    }

    private static bool Left(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathUtils_Area(ref a, ref b, ref c) > 0;
    }

    private static bool LeftOn(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathUtils_Area(ref a, ref b, ref c) >= 0;
    }

    private static bool Right(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathUtils_Area(ref a, ref b, ref c) < 0;
    }

    private static bool RightOn(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathUtils_Area(ref a, ref b, ref c) <= 0;
    }

    private static float SquareDist(Vector2 a, Vector2 b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return dx * dx + dy * dy;
    }
	
	public static Vector2 LineTools_LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        Vector2 i = Vector2.zero;
        float a1 = p2.y - p1.y;
        float b1 = p1.x - p2.x;
        float c1 = a1 * p1.x + b1 * p1.y;
        float a2 = q2.y - q1.y;
        float b2 = q1.x - q2.x;
        float c2 = a2 * q1.x + b2 * q1.y;
        float det = a1 * b2 - a2 * b1;

        if (!MathUtils_FloatEquals(det, 0))
        {
            // lines are not parallel
            i.x = (b2 * c1 - b1 * c2) / det;
            i.y = (a1 * c2 - a2 * c1) / det;
        }
        return i;
    }
	
	public static bool LineTools_LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4,
                                         bool firstIsSegment, bool secondIsSegment,
                                         out Vector2 point)
        {
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.y - point3.y;
            float b = point2.x - point1.x;
            float c = point4.x - point3.x;
            float d = point2.y - point1.y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -Mathf.Epsilon && denom <= Mathf.Epsilon))
            {
                float e = point1.y - point3.y;
                float f = point1.x - point3.x;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f || ub != 0f)
                        {
                            //There is an intersection
                            point.x = point1.x + ua * b;
                            point.y = point1.y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
	
	public static bool LineTools_LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment,bool secondIsSegment, out Vector2 intersectionPoint)
    {
        return LineTools_LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
    }
	
	public static bool LineTools_LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)
    {
        return LineTools_LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
    }
	
	public static bool LineTools_LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, out Vector2 intersectionPoint)
    {
        return LineTools_LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
    }
	
	public static Vector2[] CollinearSimplify(Vector2[] vertices, float collinearityTolerance)
    {
		int vertexCount = vertices.Length;
		
        //We can't simplify polygons under 3 vertices
        if (vertexCount < 3)
            return vertices;

        List<Vector2> simplified = new List<Vector2>(vertexCount);

        for (int i = 0; i < vertexCount; i++)
        {
            int prevId = i-1;
			if(prevId == -1) prevId = vertexCount-1;
            int nextId = i+1;
			if(nextId == vertexCount) nextId = 0;

            Vector2 prev = vertices[prevId];
            Vector2 current = vertices[i];
            Vector2 next = vertices[nextId];

            //If they collinear, continue
            if (MathUtils_Collinear(ref prev, ref current, ref next, collinearityTolerance))
                continue;

            simplified.Add(current);
        }

        return simplified.ToArray();
    }
	
	public static float MathUtils_Area(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathUtils_Area(ref a, ref b, ref c);
    }

    public static float MathUtils_Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
    {
        return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
    }

    public static bool MathUtils_Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c)
    {
        return MathUtils_Collinear(ref a, ref b, ref c, 0);
    }

    public static bool MathUtils_Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance)
    {
        return MathUtils_FloatInRange(MathUtils_Area(ref a, ref b, ref c), -tolerance, tolerance);
    }
	
	public static bool MathUtils_FloatInRange(float value, float min, float max)
    {
        return (value >= min && value <= max);
    }
	
	public static bool MathUtils_FloatEquals(float value1, float value2)
    {
        return Mathf.Abs(value1 - value2) <= Mathf.Epsilon;
    }

    public static bool MathUtils_FloatEquals(float value1, float value2, float delta)
    {
        return MathUtils_FloatInRange(value1, value2 - delta, value2 + delta);
    }

}
