using UnityEngine;
using System;
using System.Collections.Generic;


public static class RXUtils
{
	public static float GetAngle(this Vector2 vector)
	{
		return Mathf.Atan2(-vector.y, vector.x) * RXMath.RTOD;
	}

	public static float GetRadians(this Vector2 vector)
	{
		return Mathf.Atan2(-vector.y, vector.x);
	}
	public static Rect ExpandRect(Rect rect, float paddingX, float paddingY)
	{
		return new Rect(rect.x - paddingX, rect.y - paddingY, rect.width + paddingX*2, rect.height+paddingY*2);	
	}
	
	public static void LogRect(string name, Rect rect)
	{
		Debug.Log (name+": ("+rect.x+","+rect.y+","+rect.width+","+rect.height+")");	
	}
	
	public static void LogVectors(string name, params Vector2[] args)
	{
		string result = name + ": " + args.Length + " Vector2 "+ args[0].ToString()+"";

		for(int a = 1; a<args.Length; ++a)
		{
			Vector2 arg = args[a];
			result = result + ", "+ arg.ToString()+"";	
		}
		
		Debug.Log(result);
	}
	
	public static void LogVectors(string name, params Vector3[] args)
	{
		string result = name + ": " + args.Length + " Vector3 "+args[0].ToString()+"";

		for(int a = 1; a<args.Length; ++a)
		{
			Vector3 arg = args[a];
			result = result + ", "+ arg.ToString()+"";	
		}
		
		Debug.Log(result);
	}
	
	public static void LogVectorsDetailed(string name, params Vector2[] args)
	{
		string result = name + ": " + args.Length + " Vector2 "+ VectorDetailedToString(args[0])+"";
		
		for(int a = 1; a<args.Length; ++a)
		{
			Vector2 arg = args[a];
			result = result + ", "+ VectorDetailedToString(arg)+"";	
		}
		
		Debug.Log(result);
	}
	
	public static string VectorDetailedToString(Vector2 vector)
	{
		return "("+vector.x + "," + vector.y +")";
	}
	
	public static Color GetColorFromHex(uint hex)
	{
		uint red = hex >> 16;
		uint greenBlue = hex - (red<<16);
		uint green = greenBlue >> 8;
		uint blue = greenBlue - (green << 8);
		
		return new Color(red/255.0f, green/255.0f, blue/255.0f);
	}
	
	public static Color GetColorFromHex(string hexString)
	{
		return GetColorFromHex(Convert.ToUInt32(hexString,16));
	}
	
	public static Vector2 GetVector2FromString(string input)
	{
		string[] parts = input.Split(new char[] {','});	
		return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
	}
	
}

public class RXColorHSL
{
	public float h = 0.0f;
	public float s = 0.0f;
	public float l = 0.0f;
	
	public RXColorHSL(float h, float s, float l)
	{
		this.h = h;
		this.s = s;
		this.l = l;
	}
	
	public RXColorHSL() : this(0.0f, 0.0f, 0.0f) {}
}

public class RXColor
{
	//TODO: IMPLEMENT THIS
	public static Color ColorFromRGBString(string rgbString)
	{
		return Color.red;
	}
	
	//TODO: IMPLEMENT THIS
	public static Color ColorFromHSLString(string hslString)
	{
		return Color.green;
	}
	
	public static Color ColorFromHSL(RXColorHSL hsl)
	{
		return ColorFromHSL(hsl.h, hsl.s, hsl.l);
	}
	
	public static Color ColorFromHSL(float hue, float sat, float lum)
	{
		return ColorFromHSL(hue,sat,lum,1.0f);	
	}
	
	//hue goes from 0 to 1
	public static Color ColorFromHSL(float hue, float sat, float lum, float alpha) //default - sat:1, lum:0.5
	{
		hue = (100000.0f+hue)%1f; //hue wraps around
		
		float r = lum;
		float g = lum;
		float b = lum;

        float v = (lum <= 0.5f) ? (lum * (1.0f + sat)) : (lum + sat - lum * sat);
		 
        if (v > 0)
        {
			float m = lum + lum - v;
			float sv = (v - m ) / v;
			
			hue *= 6.0f;
			
			int sextant = (int) hue;
			float fract = hue - sextant;
			float vsf = v * sv * fract;
			float mid1 = m + vsf;
			float mid2 = v - vsf;
			
			switch (sextant)
			{
				case 0:
				      r = v;
				      g = mid1;
				      b = m;
				      break;
				case 1:
				      r = mid2;
				      g = v;
				      b = m;
				      break;
				case 2:
				      r = m;
				      g = v;
				      b = mid1;
				      break;
				case 3:
				      r = m;
				      g = mid2;
				      b = v;
				      break;
				case 4:
				      r = mid1;
				      g = m;
				      b = v;
				      break;
				case 5:
				      r = v;
				      g = m;
				      b = mid2;
				      break;
              }
        }
		
		return new Color(r,g,b,alpha);
	}
		
	// 
	// Math for the conversion found here: http://www.easyrgb.com/index.php?X=MATH
	//
	public static RXColorHSL HSLFromColor(Color rgb)
	{
		RXColorHSL c = new RXColorHSL();
		
		float r = rgb.r;
		float g = rgb.g;
		float b = rgb.b;
		
		float minChan = Mathf.Min(r, g, b);			//Min. value of RGB
		float maxChan = Mathf.Max(r, g, b);			//Max. value of RGB
		float deltaMax = maxChan - minChan;         //Delta RGB value
		
		c.l = (maxChan + minChan) * 0.5f;
		
		if (Mathf.Abs(deltaMax) <= 0.0001f)              //This is a gray, no chroma...
		{
			c.h = 0;								//HSL results from 0 to 1
			c.s = 0;
		}
		else										//Chromatic data...
		{
			if ( c.l < 0.5f ) 
				c.s = deltaMax / (maxChan + minChan);
			else           
				c.s = deltaMax / (2.0f - maxChan - minChan);
			
			float deltaR = (((maxChan - r) / 6.0f) + (deltaMax * 0.5f)) / deltaMax;
			float deltaG = (((maxChan - g) / 6.0f) + (deltaMax * 0.5f)) / deltaMax;
			float deltaB = (((maxChan - b) / 6.0f) + (deltaMax * 0.5f)) / deltaMax;
			
			if (Mathf.Approximately(r, maxChan)) 
				c.h = deltaB - deltaG;
			else if (Mathf.Approximately(g, maxChan)) 
				c.h = (1.0f / 3.0f) + deltaR - deltaB;
			else if (Mathf.Approximately(b, maxChan))
				c.h = (2.0f / 3.0f) + deltaG - deltaR;
			
			if (c.h < 0.0f) 
				c.h += 1.0f;
			else if (c.h > 1.0f) 
				c.h -= 1.0f;
		}
		return c;
	}

	public static Color GetColorFromHex(uint hex)
	{
		uint red = hex >> 16;
		uint greenBlue = hex - (red<<16);
		uint green = greenBlue >> 8;
		uint blue = greenBlue - (green << 8);
		
		return new Color(red/255.0f, green/255.0f, blue/255.0f);
	}
}

public class RXMath
{
	public const float RTOD = 180.0f/Mathf.PI;
	public const float DTOR = Mathf.PI/180.0f;
	public const float DOUBLE_PI = Mathf.PI*2.0f;
	public const float HALF_PI = Mathf.PI/2.0f;
	public const float PI = Mathf.PI;
	public const float INVERSE_PI = 1.0f/Mathf.PI;
	public const float INVERSE_DOUBLE_PI = 1.0f/(Mathf.PI*2.0f);
	
	public static int Wrap(int input, int range)
	{
		return (input + (range*1000000)) % range;	
	}
	
	public static float Wrap(float input, float range)
	{
		return (input + (range*1000000)) % range;	
	}
	
	
	public static float GetDegreeDelta(float startAngle, float endAngle) //chooses the shortest angular distance
	{
		float delta = (endAngle - startAngle) % 360.0f;
		
		if (delta != delta % 180.0f) 
		{
			delta = (delta < 0) ? delta + 360.0f : delta - 360.0f;
		}	
		
		return delta;
	}
	
	public static float GetRadianDelta(float startAngle, float endAngle) //chooses the shortest angular distance
	{
		float delta = (endAngle - startAngle) % DOUBLE_PI;
		
		if (delta != delta % Mathf.PI) 
		{
			delta = (delta < 0) ? delta + DOUBLE_PI : delta - DOUBLE_PI;
		}	
		
		return delta;
	}
	
	//normalized ping pong (apparently Unity has this built in... so yeah) - Mathf.PingPong()
	public static float PingPong(float input, float range)
	{
		float first = ((input + (range*1000000.0f)) % range)/range; //0 to 1
		if(first < 0.5f) return first*2.0f;
		else return 1.0f - ((first - 0.5f)*2.0f); 
	}
	
}

public static class RXRandom
{
	private static System.Random _randomSource = new System.Random();
	
	public static float Float()
	{
		return (float)_randomSource.NextDouble();
	}
	
	public static float Float(int seed)
	{
		return (float)new System.Random(seed).NextDouble();
	}
	
	public static double Double()
	{
		return _randomSource.NextDouble();
	}
	
	public static float Float(float max)
	{
		return (float)_randomSource.NextDouble() * max;
	}
	
	public static int Int()
	{
		return _randomSource.Next();
	}
	
	public static int Int(int max)
	{
		if(max == 0) return 0;
		return _randomSource.Next() % max;
	}
	
	public static float Range(float low, float high)
	{
		return low + (high-low)*(float)_randomSource.NextDouble();
	}
	
	public static int Range(int low, int high)
	{
		int delta = high - low;
		if(delta == 0) return 0;
		return low + _randomSource.Next() % delta; 
	}
	
	public static bool Bool()
	{
		return _randomSource.NextDouble() < 0.5;	
	}

	//random item from all passed arguments/params - RXRandom.Select(one, two, three);
	public static object Select(params object[] objects)
	{
		return objects[_randomSource.Next() % objects.Length];
	}

	//random item from an array
	public static T AnyItem<T>(T[] items)
	{
		if(items.Length == 0) return default(T); //null
		return items[_randomSource.Next() % items.Length];
	}

	//random item from a list
	public static T AnyItem<T>(List<T> items)
	{
		if(items.Count == 0) return default(T); //null
		return items[_randomSource.Next() % items.Count];
	}
	
	//this isn't really perfectly randomized, but good enough for most purposes
	public static Vector2 Vector2Normalized()
	{
		return new Vector2(RXRandom.Range(-1.0f,1.0f),RXRandom.Range(-1.0f,1.0f)).normalized;
	}
	
	public static Vector3 Vector3Normalized()
	{
		return new Vector3(RXRandom.Range(-1.0f,1.0f),RXRandom.Range(-1.0f,1.0f),RXRandom.Range(-1.0f,1.0f)).normalized;
	}

	public static void ShuffleList<T>(List<T> list)
	{
		list.Sort(RandomComparison);
	}

	public static void Shuffle<T>(this List<T> list)
	{
		list.Sort(RandomComparison);
	}

	private static int RandomComparison<T>(T a, T b) 
	{
		if(_randomSource.Next() % 2 == 0) return -1;

		return 1;
	}

}

public class RXCircle
{
	public Vector2 center;
	public float radius;
	public float radiusSquared;
	
	public RXCircle(Vector2 center, float radius)
	{
		this.center = center;
		this.radius = radius;
		this.radiusSquared = radius * radius;
	}
	
	public bool CheckIntersectWithRect(Rect rect)
	{
		return rect.CheckIntersectWithCircle(this);
	}
	
	public bool CheckIntersectWithCircle(RXCircle circle)
	{
		Vector2 delta = circle.center - this.center;
		float radiusSumSquared = (circle.radius + this.radius) * (circle.radius + this.radius);
		return (delta.sqrMagnitude <= radiusSumSquared);
	}
}

//This class is incomplete, I just have to get around to converting all the equations to this simplified format
public static class RXEase
{
	//based on GoKit's easing equations: https://github.com/prime31/GoKit/tree/master/Assets/Plugins/GoKit/easing
	//but simplified to work with only normalized values (0..1)
	//t = current time, b = starting value, c = final value, d = duration
	//for our purposes, t = input, b = 0, d = 1, c = 1 :::: note that (t/d = input)

	public static float QuadOut(float input)
	{
		return -input * (input - 2.0f);
	}

	public static float QuadIn(float input)
	{
		return input * input;
	}

	public static float QuadInOut(float input)
	{
		if (input < 0.5f) return 2.0f * input * input;
		input = (input-0.5f) * 2.0f;
		return 0.5f - 0.5f * input * (input - 2.0f);
	}

	public static float ExpoOut(float input)
	{
		return -Mathf.Pow( 2.0f, -10.0f * input) + 1.0f;
	}

	public static float ExpoIn(float input)
	{
		return Mathf.Pow(2.0f,10.0f * (input - 1.0f));
	}

	public static float ExpoInOut(float input)
	{
		if (input < 0.5f) return Mathf.Pow(2.0f,10.0f * (input*2.0f - 1.0f)) * 0.5f;
		else return 0.5f + (-Mathf.Pow( 2.0f, -20.0f * (input-0.5f)) + 1.0f) * 0.5f;
	}
	
	public static float BackOut(float input) {return BackOut(input,1.7f);}
	public static float BackOut(float input, float backAmount)
	{
		input = input - 1.0f;
		return (input * input * ((backAmount + 1) * input + backAmount) + 1);
	}

	public static float BackIn(float input) {return BackIn(input,1.7f);}
	public static float BackIn(float input, float backAmount)
	{
		return  input * input * ((backAmount + 1.0f) * input - backAmount);
	}

	public static float BackInOut(float input) {return BackInOut(input,1.7f);}
	public static float BackInOut(float input, float backAmount)
	{
		if (input < 0.5f) return BackIn(input*2.0f,backAmount)*0.5f;
		else return 0.5f + BackOut((input-0.5f)*2.0f,backAmount)*0.5f;
	}

	public static float SinInOut(float input)
	{
		return -0.5f * (Mathf.Cos(Mathf.PI*input) - 1.0f);
	}
}


