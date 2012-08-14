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
	
	//XT.To(target,0.5f).PropFloat("x",100.0f).OnComplete(OnCompleteHandler).Delay(0.5f).Start();
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
	
	public static Rect Multiply(Rect rect, float multiplier)
	{
		rect.x *= multiplier;
		rect.y *= multiplier;
		rect.width *= multiplier;
		rect.height *= multiplier;
		return rect;
	}
}

public static class GoKitExtensions
{
	//this makes it so we don't have to specify false for isRelative every.single.time.
	public static TweenConfig floatProp(this TweenConfig config, string propName, float propValue)
	{
		return config.floatProp(propName,propValue,false); 
	}
}

public static class ListExtensions
{
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