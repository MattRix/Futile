using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
//
//public static class EventExtensions
//{
//    
//}

public static class RXColorExtensions
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

public static class RXRectExtensions
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

    //this can handle rects with negative width and negative height
    public static bool CheckIntersectComplex(this Rect rect, Rect otherRect)
    {
        float rx = rect.x;
        float ry = rect.y;

        float orx = otherRect.x;
        float ory = otherRect.y;
        
        return 
        (
            Mathf.Max(rx, rx + rect.width) >= Mathf.Min(orx, orx + otherRect.width) && 
            Mathf.Min(rx, rx + rect.width) <= Mathf.Max(orx, orx + otherRect.width) && 
            Mathf.Max(ry, ry + rect.height) >= Mathf.Max(ory, ory + otherRect.height) && 
            Mathf.Min(ry, ry + rect.height) <= Mathf.Min(ory, ory + otherRect.height)
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

	public static Rect CloneAndScale(this Rect rect, float scaleX, float scaleY)
	{
		rect.x = rect.x*scaleX;
		rect.y = rect.y*scaleY;
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

public static class RXGoKitExtensions
{
	//this makes it so we don't have to specify false for isRelative every.single.time.
	public static TweenConfig floatProp(this TweenConfig config, string propName, float propValue)
	{
		return config.floatProp(propName,propValue,false); 
	}

	//this makes it so we don't have to specify false for isRelative every.single.time.
	public static TweenConfig colorProp(this TweenConfig config, string propName, Color propValue)
	{
		return config.colorProp(propName,propValue,false); 
	}

	public static TweenConfig removeWhenComplete(this TweenConfig config)
	{
		config.onComplete((tween) => {((tween as Tween).target as FNode).RemoveFromContainer();});	
		return config;
	}

	public static TweenConfig hideWhenComplete(this TweenConfig config)
	{
		config.onComplete((tween) => {((tween as Tween).target as FNode).isVisible = false;});	
		return config;
	}

	//forward to an action with no arguments instead (ex you can pass myNode.RemoveFromContainer)
	public static TweenConfig onComplete(this TweenConfig config, Action onCompleteAction)
	{
		config.onComplete((tween) => {onCompleteAction();});	
		return config;
	}

	public static TweenConfig alpha(this TweenConfig config, float alpha)
	{
		config.tweenProperties.Add(new FloatTweenProperty("alpha",alpha,false));
		return config;
	}

	public static TweenConfig rotation(this TweenConfig config, float rotation)
	{
		config.tweenProperties.Add(new FloatTweenProperty("rotation",rotation,false));
		return config;
	}

	public static TweenConfig x(this TweenConfig config, float x)
	{
		config.tweenProperties.Add(new FloatTweenProperty("x",x,false));
		return config;
	}

	public static TweenConfig y(this TweenConfig config, float y)
	{
		config.tweenProperties.Add(new FloatTweenProperty("y",y,false));
		return config;
	}

	public static TweenConfig pos(this TweenConfig config, float x,float y)
	{
		config.tweenProperties.Add(new FloatTweenProperty("x",x,false));
		config.tweenProperties.Add(new FloatTweenProperty("y",y,false));
		return config;
	}

	public static TweenConfig pos(this TweenConfig config, Vector2 pos)
	{
		config.tweenProperties.Add(new FloatTweenProperty("x",pos.x,false));
		config.tweenProperties.Add(new FloatTweenProperty("y",pos.y,false));
		return config;
	}

	public static TweenConfig scaleXY(this TweenConfig config, float scaleX,float scaleY)
	{
		config.tweenProperties.Add(new FloatTweenProperty("scaleX",scaleX,false));
		config.tweenProperties.Add(new FloatTweenProperty("scaleY",scaleY,false));
		return config;
	}
	
	public static TweenConfig scaleXY(this TweenConfig config, float scale)
	{
		config.tweenProperties.Add(new FloatTweenProperty("scale",scale,false));
		return config;
	}

	public static TweenConfig backOut(this TweenConfig config)
	{
		config.easeType = EaseType.BackOut;
		return config;
	}

	public static TweenConfig backIn(this TweenConfig config)
	{
		config.easeType = EaseType.BackIn;
		return config;
	}

	public static TweenConfig expoOut(this TweenConfig config)
	{
		config.easeType = EaseType.ExpoOut;
		return config;
	}

	public static TweenConfig expoIn(this TweenConfig config)
	{
		config.easeType = EaseType.ExpoIn;
		return config;
	}

	public static TweenConfig expoInOut(this TweenConfig config)
	{
		config.easeType = EaseType.ExpoInOut;
		return config;
	}

}

public static class RXArrayExtensions
{
	public static int IndexOf<T>(this T[] items, T itemToFind) where T:class
	{
		int count = items.Length;

		for(int i = 0; i<count; i++)
		{
			if(items[i] == itemToFind)
			{
				return i;
			}
		}	
		return -1;
	}
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

	public static void Log<T>(this T[] items) {items.Log("");}
	public static void Log<T>(this T[] items, string name)
	{
		StringBuilder builder = new StringBuilder();

		if(name != "")
		{
			builder.Append(name);
			builder.Append(": ");
		}

		builder.Append('[');

		int count = items.Length;

		for(int t = 0;t<count;t++)
		{
			builder.Append(items[t].ToString());
			if(t < count-1) builder.Append(',');
		}

		builder.Append(']');

		Debug.Log(builder.ToString());
	}
}

public static class RXListExtensions
{
	public static void Log<T>(this List<T> list) {list.Log("");}
	public static void Log<T>(this List<T> list, string name)
	{
		StringBuilder builder = new StringBuilder();

		if(name != "")
		{
			builder.Append(name);
			builder.Append(": ");
		}

		builder.Append('[');

		int count = list.Count;

		for(int t = 0;t<count;t++)
		{
			builder.Append(list[t].ToString());
			if(t < count-1) builder.Append(',');
		}

		builder.Append(']');

		Debug.Log(builder.ToString());
	}

	//adds item to the start of the list
	public static void Unshift<T>(this List<T> list, T item)
	{
		list.Insert(0,item);
	}

	//removes first item from a list and returns it 
	public static T Shift<T>(this List<T> list)
	{
		T item = list[0];
		list.RemoveAt(0);
		return item;
	}

	//adds item to the end of the list (note: I recommend using .Add(), this is just here for completeness)
	public static void Push<T>(this List<T> list, T item)
	{
		list.Add(item);
	}

	//removes last item from a list and returns it
	public static T Pop<T>(this List<T> list)
	{
		T item = list[list.Count-1];
		list.RemoveAt(list.Count-1);
		return item;
	}

	public static T GetFirstItem<T>(this List<T> list)
	{
		return list[0];
	}

	public static T GetLastItem<T>(this List<T> list)
	{
		return list[list.Count-1];
	}
	
	
	//insertion sort is stable, in other words, equal items will stay in the same order (unlike List.Sort, which uses QuickSort)
	//this could be replaced with a MergeSort, which should be more efficient in a lot of cases
	//basic implementation from http://www.csharp411.com/c-stable-sort/
	public static bool InsertionSort<T>(this List<T> list, Comparison<T> comparison)
	{
		bool didChange = false;
	    int count = list.Count;
		
	    for (int j = 1; j < count; j++)
	    {
	        T item = list[j];
	
	        int i = j - 1;
			
	        for (; i >= 0 && comparison( list[i], item ) > 0; i--)
	        {
	            list[i + 1] = list[i];
				didChange = true;
	        }
			
	        list[i + 1] = item;
    	}

		return didChange;
	}
}

public static class RXDictionaryExtensions
{
	public static int GetInt(this Dictionary<string,object> dict, string key, int defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return int.Parse(result.ToString());
		return defaultValue;
	}

	public static long GetLong(this Dictionary<string,object> dict, string key, long defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return long.Parse(result.ToString());
		return defaultValue;
	}

	public static float GetFloat(this Dictionary<string,object> dict, string key, float defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return float.Parse(result.ToString());
		return defaultValue;
	}

	public static double GetDouble(this Dictionary<string,object> dict, string key, double defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return double.Parse(result.ToString());
		return defaultValue;
	}

	public static bool GetBool(this Dictionary<string,object> dict, string key, bool defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return bool.Parse(result.ToString());
		return defaultValue;
	}

	public static string GetString(this Dictionary<string,object> dict, string key, string defaultValue)
	{
		object result;
		if(dict.TryGetValue(key,out result)) return result.ToString();
		return defaultValue;
	}

	public static int GetInt(this Dictionary<string,string> dict, string key, int defaultValue)
	{
		string result;
		if(dict.TryGetValue(key,out result)) return int.Parse(result.ToString());
		return defaultValue;
	}

	public static long GetLong(this Dictionary<string,string> dict, string key, long defaultValue)
	{
		string result;
		if(dict.TryGetValue(key,out result)) return long.Parse(result.ToString());
		return defaultValue;
	}

	//this implementation can alternatively be written using dict.TryGetValue, but I'm not sure which approach is faster
	public static TValue GetValueOrDefault<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, TValue defaultValue)
	{
		if(dict.ContainsKey(key))
		{
			return dict[key];
		}
		else 
		{
			return defaultValue;
		}
	}
	
	public static void SetValueIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref TValue thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = dict[key];
		}
	}
	
	//these next few are super handy for parsing json Dictionary<string,object> stuff
	
	public static void SetStringIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref string thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = dict[key].ToString();
		}
	}
	
	public static void SetFloatIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref float thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = float.Parse(dict[key].ToString());
		}
	}

	public static void SetDoubleIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref double thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = double.Parse(dict[key].ToString());
		}
	}
	
	public static void SetIntIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref int thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = int.Parse(dict[key].ToString());
		}
	}

	public static void SetLongIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref long thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = long.Parse(dict[key].ToString());
		}
	}
	
	public static void SetBoolIfExists<TKey,TValue>(this Dictionary<TKey,TValue> dict, TKey key, ref bool thingToSet)
	{
		if(dict.ContainsKey(key))
		{
			thingToSet = bool.Parse(dict[key].ToString());
		}
	}

	public static void LogDetailed<TKey,TValue>(this Dictionary<TKey,TValue> dict, string name)
	{
		foreach(var kv in dict)
		{
			Debug.Log(name+"["+kv.Key.ToString()+"] = " + kv.Value.ToString());
		}
	}
}

public static class RXStringExtensions
{
	public static string Format(this string @this, params object[] args)
	{
		return string.Format(@this,args);
	}

	public static string ToUpperFirstLetter(this string @this)
	{
		return char.ToUpper(@this[0]) + @this.Substring(1).ToLower();
	}
}

public static class RXIntExtensions
{
	public static string PluralS(this int @this)
	{
		return (@this == 1 ? "" : "s");
	}
}

