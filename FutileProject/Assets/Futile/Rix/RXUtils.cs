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

	//a bit of a hacky (and very slow) way to do it, but it makes it readable enough for debugging purposes
	public static string PrettyifyJson(string output)
	{
		output = output.Replace("{","\n{\n");
		output = output.Replace("}","\n}");
		output = output.Replace("[","\n[\n");
		output = output.Replace("]","\n]");
		output = output.Replace(",",",\n");
		return output;
	}

	public static bool AreListsEqual<T>(List<T> listA, List<T> listB)
	{
		if(listA.Count != listB.Count)
		{
			return false;
		}

		int count = listA.Count;

		for(int c = 0; c<count; c++)
		{
			if(!listA[c].Equals(listB[c])) return false;
		}

		return true;
	}
}

public static class RXArrayUtil
{
	public static T[] CreateArrayFilledWithItem<T> (T item, int count)
	{
		T[] result = new T[count];
		for(int c = 0; c<count; c++)
		{
			result[c] = item;
		}
		return result;
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

	//Mod is basically a version of mod (%) that works with negative numbers
	public static int Mod(int input, int range)
	{
		int result = input % range;
		return (result < 0) ? result + range : result;
	}

	//Mod is basically a version of mod (%) that works with negative numbers
	public static float Mod(float input, float range) 
	{
		float result = input % range;
		return (result < 0) ? result + range : result;
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

	//turns input from 0 to 1 into a saw pattern from 0 to 1 and back to 0... so when input is 0.5, the output is 1 etc.
	public static float Saw(float input)
	{
		input = Mod(input,1.0f);
		if(input < 0.5f) return input*2f;
		return 2f-input*2f; 
	}

	//turn input from 0 to 1 into a circular sin pattern
	public static float Circ(float input)
	{
		input = Mod(input,1.0f);
		return Mathf.Sin(input * Mathf.PI);
	}

	public static Vector2 GetOffsetFromAngle(float angle, float distance)
	{
		float radians = angle * RXMath.DTOR;
		return new Vector2(Mathf.Cos(radians) * distance, -Mathf.Sin(radians) * distance);
	}

	//returns the percentage across a subrange... 
	//so for example (0.75,0.25,1.0) would return 0.666, because 0.75f is 66% of the way between 0.25 and 1.0f
	public static float GetSubPercent(float fullPercent, float lowEnd, float highEnd)
	{
		return Mathf.Clamp01((fullPercent-lowEnd)/(highEnd-lowEnd));
	}
	public static int RoundUpToNearest(float source, float roundAmount)
	{
		return (int)(Mathf.Ceil(source/roundAmount)*roundAmount); 
	}
}

public static class RXRandom
{
	private static Stack<System.Random> _randomSources = new Stack<System.Random>();
	private static System.Random _randomSource = new System.Random();

	public static void PushSeed()
	{
		_randomSources.Push(_randomSource);//add the old one to the stack
		_randomSource = new System.Random();
	}

	public static void PushSeed(int seed)
	{
		_randomSources.Push(_randomSource);
		_randomSource = new System.Random(seed);
	}

	public static void PopSeed()
	{
		if(_randomSources.Count > 0) 
		{
			//remove the last randomSource nad make it the new _randomSource
			_randomSource = _randomSources.Pop();
		}
	}

	public static void ResetSeed()
	{
		while(_randomSources.Count > 0) 
		{
			//remove the last randomSource nad make it the new _randomSource
			_randomSource = _randomSources.Pop();
		}
	}

	public static float Float()
	{
		return (float)_randomSource.NextDouble();
	}
	
	public static float SeedFloat(int seed)
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

	public static int SeedInt(int seed)
	{
		return new System.Random(seed).Next();
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

	public static float SeedRange(int seed, float low, float high)
	{
		return low + (high-low)*(float)new System.Random(seed).NextDouble();
	}

	//note, this will never return the high value (so if you enter Range(0,2) you'll only get 0 and 1)
	public static int Range(int low, int high)
	{
		int delta = high - low;
		if(delta == 0) return low;
		return low + _randomSource.Next() % delta; 
	}

	//note, this will never return the high value (so if you enter Range(0,2) you'll only get 0 and 1)
	public static int SeedRange(int seed, int low, int high)
	{
		int delta = high - low;
		if(delta == 0) return low;
		return low + new System.Random(seed).Next() % delta; 
	}
	
	public static bool Bool()
	{
		return _randomSource.NextDouble() < 0.5;	
	}

	public static bool Bool(float chanceOfTrue)
	{
		return _randomSource.NextDouble() < chanceOfTrue;	
	}

	//random item from all passed arguments/params - RXRandom.Select(one, two, three);
	public static object GetRandomItem(params object[] objects)
	{
		return objects[_randomSource.Next() % objects.Length];
	}

	public static string GetRandomString(params string[] strings)
	{
		return strings[_randomSource.Next() % strings.Length];
	}
	public static Color Color(float alpha)
	{
		return new Color(RXRandom.Float(),RXRandom.Float(),RXRandom.Float(),alpha);
	}
	//random item from an array
	public static T GetRandomItem<T>(T[] items)
	{
		if(items.Length == 0) return default(T); //null
		return items[_randomSource.Next() % items.Length];
	}

	//replaces one item at random with a new one 
	public static void ReplaceRandomItem<T>(T[] items, T item)
	{
		items[_randomSource.Next() % items.Length] = item;
	}

	//random item from a list
	public static T GetRandomItem<T>(List<T> items)
	{
		if(items.Count == 0) return default(T); //null
		return items[_randomSource.Next() % items.Count];
	}

	//replaces one item at random with a new one 
	public static void ReplaceRandomItem<T>(List<T> items, T item)
	{
		items[_randomSource.Next() % items.Count] = item;
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

	public static int RandomComparison<T>(T a, T b) 
	{
		if(_randomSource.Next() % 2 == 0) return -1;

		return 1;
	}

	public const string randomChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

	public static string GenerateRandomString(int numChars)
	{
		string result = "";
		for(int n = 0; n<numChars; n++)
		{
			result += randomChars[(_randomSource.Next() % randomChars.Length)];
		}
		return result;
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

//these equations shamelessly stolen from Chevy Ray's AutoMotion ;) (https://github.com/UnityPatterns/AutoMotion/) 
//note that they only take a t variable (which should be between 0 and 1) and return a value between 0 and 1
public static class RXEase
{
	public delegate float Dele(float t);

	public static Dele Linear = (t) => 		{ return t; };
	public static Dele QuadIn = (t) => 		{ return t * t; };
	public static Dele QuadOut = (t) => 	{ return 1f - QuadIn(1f - t); };
	public static Dele QuadInOut = (t) => 	{ return (t <= 0.5f) ? QuadIn(t * 2f) * 0.5f : QuadOut(t * 2f - 1f) * 0.5f + 0.5f; };
	public static Dele CubeIn = (t) => 		{ return t * t * t; };
	public static Dele CubeOut = (t) => 	{ return 1f - CubeIn(1f - t); };
	public static Dele CubeInOut = (t) => 	{ return (t <= 0.5f) ? CubeIn(t * 2f) * 0.5f : CubeOut(t * 2f - 1f) * 0.5f + 0.5f; };
	public static Dele BackIn = (t) => 		{ return t * t * (2.70158f * t - 1.70158f); };
	public static Dele BackOut = (t) => 	{ return 1f - BackIn(1f - t); };
	public static Dele BackInOut = (t) => 	{ return (t <= 0.5f) ? BackIn(t * 2f) * 0.5f : BackOut(t * 2f - 1f) * 0.5f + 0.5f; };
	public static Dele ExpoIn = (t) => 		{ return Mathf.Pow(2f, 10f * (t-1.0f)); };
	public static Dele ExpoOut = (t) => 	{ return 1f - Mathf.Pow(2f, -10f * t); };
	public static Dele ExpoInOut = (t) => 	{ return t < .5f ? ExpoIn(t * 2f) * 0.5f : ExpoOut(t * 2f - 1f) * 0.5f + 0.5f; };
	public static Dele SineIn = (t) => 		{ return -Mathf.Cos(Mathf.PI * 0.5f * t) + 1f; };
	public static Dele SineOut = (t) => 	{ return Mathf.Sin(Mathf.PI * 0.5f * t); };
	public static Dele SineInOut = (t) => 	{ return -Mathf.Cos(Mathf.PI * t) * 0.5f + .5f; };
	public static Dele ElasticIn = (t) => 	{ return 1f - ElasticOut(1f - t); };
	public static Dele ElasticOut = (t) => 	{ return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.075f) * (2f * Mathf.PI) / 0.3f) + 1f; };
	public static Dele ElasticInOut = (t) =>{ return (t <= 0.5f) ? ElasticIn(t * 2f) / 2f : ElasticOut(t * 2f - 1f) * 0.5f + 0.5f; };

	//turns input from 0 to 1 into a eased saw pattern from 0 to 1 and back to 0... so when input is 0.5, the output is 1 etc.
	public static float UpDown(float input, Dele easeFunc)
	{
		if(input < 0.5f) return easeFunc(input*2f);
		return easeFunc(2f-input*2f); 
	}

}


//converts the simple equations in RXEase into the standard t b c d format
//where t = current time, b = starting value, c = final value, d = duration
//(I don't really have a great use case for this, but it was a fun class to make :D)
public static class RXEaseStandard
{
	public static Dele Linear = 		Standardize(RXEase.Linear);
	public static Dele QuadIn = 		Standardize(RXEase.QuadIn);
	public static Dele QuadOut = 		Standardize(RXEase.QuadOut);
	public static Dele QuadInOut = 		Standardize(RXEase.QuadInOut);
	public static Dele CubeIn = 		Standardize(RXEase.CubeIn);
	public static Dele CubeOut = 		Standardize(RXEase.CubeOut);
	public static Dele CubeInOut = 		Standardize(RXEase.CubeInOut);
	public static Dele BackIn = 		Standardize(RXEase.BackIn);
	public static Dele BackOut = 		Standardize(RXEase.BackOut);
	public static Dele BackInOut = 		Standardize(RXEase.BackInOut);
	public static Dele ExpoIn = 		Standardize(RXEase.ExpoIn);
	public static Dele ExpoOut = 		Standardize(RXEase.ExpoOut);
	public static Dele ExpoInOut = 		Standardize(RXEase.ExpoInOut);
	public static Dele SineIn = 		Standardize(RXEase.SineIn);
	public static Dele SineOut = 		Standardize(RXEase.SineOut);
	public static Dele SineInOut = 		Standardize(RXEase.SineInOut);
	public static Dele ElasticIn = 		Standardize(RXEase.ElasticIn);
	public static Dele ElasticOut = 	Standardize(RXEase.ElasticOut);
	public static Dele ElasticInOut = 	Standardize(RXEase.ElasticInOut);
	
	public delegate float Dele(float currentTime,float startingValue,float finalValue,float duration);

	public static Dele Standardize(RXEase.Dele simpleFunc)
	{
		return (t,b,c,d) =>
		{
			return c * simpleFunc(t) / d + b;
		};
	}
}

//a handy class for keeping tweened values encapsulated
public class RXTweenable
{
	private float _amount;

	public Action SignalChange;

	public RXTweenable(float amount)
	{
		_amount = amount;
	}

	public RXTweenable(float amount, Action SignalChange)
	{
		this.SignalChange = SignalChange;
	}

	public float amount
	{
		get {return _amount;}
		set 
		{
			if(_amount != value)
			{
				_amount = value; 
				if(SignalChange != null) SignalChange();
			}
		}
	}

	public void To(float targetAmount, float duration)
	{
		this.To(targetAmount,duration, new TweenConfig());
	}

	public void To(float targetAmount, float duration, TweenConfig tc)
	{
		Go.killAllTweensWithTarget(this);
		tc.floatProp("amount", targetAmount);
		Go.to(this,duration,tc);
	}
	public static RXTweenable DelayAction(float delay, Action action)
	{
		RXTweenable rt = new RXTweenable(0.0f);
		rt.To(1.0f,delay,new TweenConfig().onComplete((t)=>{action();}));
		return rt;
	}
}

//the GoKit tweenchain was causing errors so I decided to make a simpler one
public class RXTweenChain
{
	public List<Tween> tweensToDo = new List<Tween>();

	public RXTweenChain()
	{
	}

	public RXTweenChain Add(object target, float duration, TweenConfig config)
	{
		config.onComplete(HandleTweenComplete);
		Tween tween = new Tween(target,duration,config,null);
		tweensToDo.Add(tween);
		return this;
	}

	void HandleTweenComplete(AbstractTween tween)
	{
		StartNextTween();
	}

	public void Play()
	{
		StartNextTween();
	}

	public void StartNextTween()
	{
		if(tweensToDo.Count == 0) return;
		Tween nextTween = tweensToDo.Shift();
		Go.addTween(nextTween);
		nextTween.play();
	}
}

public class BoxedLong
{
	public long value;
	public BoxedLong(long value) {this.value = value;}
}

public class BoxedInt
{
	public int value;
	public BoxedInt(int value) {this.value = value;}
}

public class BoxedFloat
{
	public float value;
	public BoxedFloat(float value) {this.value = value;}
}

public class BoxedDouble
{
	public double value;
	public BoxedDouble(double value) {this.value = value;}
}

public class BoxedBool
{
	public bool value;
	public BoxedBool(bool value) {this.value = value;}
}
