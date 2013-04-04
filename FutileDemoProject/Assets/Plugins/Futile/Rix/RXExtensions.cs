using System;
using UnityEngine;
using System.Collections.Generic;
//
//public static class EventExtensions
//{
//    
//}

public static class ColorExtensions
{
	public static Color CloneWithNewAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);	
	}
	
	public static Color CloneWithMultipliedAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, color.a*alpha);	
	}
	
	public static void ApplyMultipliedAlpha(this Color color, ref Color targetColor, float alpha)
	{
		targetColor.r = color.r;
		targetColor.g = color.g;
		targetColor.b = color.b;
		targetColor.a = color.a*alpha;
	}
}

public static class RectExtensions
{
	public static Rect CloneWithExpansion(this Rect rect, float expansionAmount)
	{
		return new Rect(rect.x - expansionAmount, rect.y - expansionAmount, rect.width + expansionAmount*2, rect.height + expansionAmount*2);
	}
	
	public static bool CheckIntersect(this Rect rect, Rect otherRect)
	{
		return 
		(
			rect.xMax >= otherRect.xMin && 
			rect.xMin <= otherRect.xMax && 
			rect.yMax >= otherRect.yMin && 
			rect.yMin <= otherRect.yMax
		);
	}
	
	public static Rect CloneAndMultiply(this Rect rect, float multiplier)
	{
		rect.x *= multiplier;
		rect.y *= multiplier;
		rect.width *= multiplier;
		rect.height *= multiplier;
		return rect;
	}
	
	public static Rect CloneAndOffset(this Rect rect, float offsetX, float offsetY)
	{
		rect.x += offsetX;
		rect.y += offsetY;
		return rect;
	}
	
	public static Rect CloneAndScaleThenOffset(this Rect rect, float scaleX, float scaleY, float offsetX, float offsetY)
	{
		rect.x = rect.x*scaleX + offsetX;
		rect.y = rect.y*scaleY + offsetY;
		rect.width *= scaleX;
		rect.height *= scaleY;
		return rect;
	}
	
	//general idea from here: http://stackoverflow.com/questions/1343346/calculate-a-vector-from-the-center-of-a-square-to-edge-based-on-radius
	//(but greatly cleaned up and simplified)
	//this is different from GetPointLimitedToInterior because it takes the angle into consideration
	public static Vector2 GetClosestInteriorPointAlongDeltaVector(this Rect rect, Vector2 targetPoint)
	{
		//if it's inside the rect, don't do anything
		if(	targetPoint.x >= rect.xMin &&
			targetPoint.x <= rect.xMax &&
			targetPoint.y >= rect.yMin &&
			targetPoint.y <= rect.yMax) return targetPoint;
		
		targetPoint.Normalize();
		
		float absX = Mathf.Abs(targetPoint.x);
		float absY = Mathf.Abs(targetPoint.y);
		float halfWidth = rect.width*0.5f;
		float halfHeight = rect.height*0.5f;
		
		if (halfWidth*absY <= halfHeight*absX)
		{
			return targetPoint * halfWidth/absX;
		}
		else
		{
			return targetPoint * halfHeight/absY;
		}
	}
	
	//this simply ensures that none of the point values are over the max and min
	public static Vector2 GetClosestInteriorPoint(this Rect rect, Vector2 targetPoint)
	{
		return new Vector2
		(
				Mathf.Clamp(targetPoint.x, rect.xMin, rect.xMax),
				Mathf.Clamp(targetPoint.y, rect.yMin, rect.yMax)
		);
	}
	
	// NOTE: Rect MUST be axis-aligned for this check
	public static bool CheckIntersectWithCircle(this Rect rect, RXCircle circle)
	{
		// Find the closest point to the circle center that's within the rectangle
		Vector2 closest = GetClosestInteriorPoint(rect, circle.center);
		
		// Calculate the distance between the circle's center and this closest point
		Vector2 deltaToClosest = circle.center - closest;
		
		// If the distance is less than the circle's radius, an intersection occurs
		bool intersection = (deltaToClosest.sqrMagnitude <= circle.radiusSquared);
		return intersection;
	}
}

public static class GoKitExtensions
{
	//this makes it so we don't have to specify false for isRelative every.single.time.
	public static TweenConfig floatProp(this TweenConfig config, string propName, float propValue)
	{
		return config.floatProp(propName,propValue,false); 
	}
	
	public static TweenConfig removeWhenComplete(this TweenConfig config)
	{
		config.onComplete(HandleRemoveWhenDoneTweenComplete);	
		return config;
	}
	
	private static void HandleRemoveWhenDoneTweenComplete (AbstractTween tween)
	{
		((tween as Tween).target as FNode).RemoveFromContainer();
	}
}

public static class ArrayExtensions
{
	public static void RemoveItem<T>(this T[] items, T itemToRemove, ref int count) where T : class
	{
		//this thing basically just removes it from the array
		bool wasFound = false;
		
		for(int i = 0; i<count; i++)
		{
			if(wasFound)
			{
				T item = items[i];
				items[i-1] = item;
			}
			else if(items[i] == itemToRemove)
			{
				wasFound = true;
			}
		}	
		
		if(wasFound) count--;
	}

}

public static class ListExtensions
{
	public static T Unshift<T>(this List<T> list)
	{
		T thing = list[0];
		list.RemoveAt(0);
		return thing;
	}
	
	public static T Pop<T>(this List<T> list)
	{
		T thing = list[list.Count-1];
		list.RemoveAt(list.Count-1);
		return thing;
	}
	
	public static T GetLastObject<T>(this List<T> list)
	{
		return list[list.Count-1];
	}
	
	//insertion sort is stable, in other words, equal items will stay in the same order (unlike List.Sort, which uses QuickSort)
	//this could be replaced with a MergeSort, which should be more efficient in a lot of cases
	//basic implementation from http://www.csharp411.com/c-stable-sort/
	public static void InsertionSort<T>(this List<T> list, Comparison<T> comparison)
	{
	    int count = list.Count;
		
	    for (int j = 1; j < count; j++)
	    {
	        T item = list[j];
	
	        int i = j - 1;
			
	        for (; i >= 0 && comparison( list[i], item ) > 0; i--)
	        {
	            list[i + 1] = list[i];
	        }
			
	        list[i + 1] = item;
    	}
	}
}