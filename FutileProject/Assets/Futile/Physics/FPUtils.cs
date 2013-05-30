using UnityEngine;
using System.Collections.Generic;
 
public static class FPUtils
{
	
	//look into poly decomposition here: http://xnaer.com/Code/Details/1548
	//http://www.box2d.org/forum/viewtopic.php?f=3&t=4395&start=30
	
	//based on http://debian.fmi.uni-sofia.bg/~sergei/cgsr/docs/clockwise.htm
	
	public static bool CheckIfConvex (Vector2[] sourceVertices) //returns true if the verts make a complex polygon
	{
		int vertCount = sourceVertices.Length;
		int i,j,k;
		int flag = 0;
		double z;
		
		if (vertCount < 3) return true; //failed
		
		for (i=0;i<vertCount;i++) 
		{
			j = (i + 1) % vertCount;
			k = (i + 2) % vertCount;
			z  = (sourceVertices[j].x - sourceVertices[i].x) * (sourceVertices[k].y - sourceVertices[j].y);
			z -= (sourceVertices[j].y - sourceVertices[i].y) * (sourceVertices[k].x - sourceVertices[j].x);
			
			if (z < 0) flag |= 1;
			else if (z > 0) flag |= 2;
			if (flag == 3) return false; //concave
		}
		
		if (flag != 0) return true; //convex
		else return true; //failed
	}
	
	//based on: http://wiki.unity3d.com/index.php?title=Triangulator (but with some modifications for increased performance)
	
    public static int[] Triangulate(Vector2[] points) 
	{
        List<int> indices = new List<int>();
 
        int n = points.Length;
        if (n < 3)
            return indices.ToArray();
 
        int[] V = new int[n];
        if (Triangulator_Area(points) > 0) 
		{
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else 
		{
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }
 
        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2; ) 
		{
            if ((count--) <= 0)
                return indices.ToArray();
 
            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;
 
            if (Triangulator_Snip(points, u, v, w, nv, V)) 
			{
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }
 
        indices.Reverse();
        return indices.ToArray();
    }
 
    static private float Triangulator_Area (Vector2[] points) 
	{
        int n = points.Length;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++) 
		{
            Vector2 pval = points[p];
            Vector2 qval = points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }
 
    static private bool Triangulator_Snip (Vector2[] points, int u, int v, int w, int n, int[] V) 
	{
        int p;
        Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++) 
		{
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = points[V[p]];
            if (Triangulator_InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }
 
   static private bool Triangulator_InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P) 
	{
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;
 
        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;
 
        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;
 
        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
	static public Vector2 ToVector2InPoints(this Vector3 vec)
	{
		return new Vector2(vec.x * FPhysics.METERS_TO_POINTS, vec.y * FPhysics.METERS_TO_POINTS);
	}
	static public Vector3 ToVector3InMeters(this Vector2 vec)
	{
		return new Vector3(vec.x * FPhysics.POINTS_TO_METERS, vec.y * FPhysics.POINTS_TO_METERS, 0.0f);
	}
}