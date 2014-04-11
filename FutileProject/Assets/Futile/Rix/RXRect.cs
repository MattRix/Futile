using UnityEngine;
using System;
using System.Collections.Generic;

//a rect that's a class
//so it's much faster than Unity's built in Rect in many circumstances

public class RXRect
{
	public float x;
	public float y;
	public float width;
	public float height;
	
	public RXRect(float x, float y, float width, float height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}
	
	public RXRect()
	{
		
	}

	public bool CheckIntersect(RXRect otherRect)
	{
		return 
		(
			((this.x+this.width) >= otherRect.x) && 
			(this.x <= (otherRect.x+otherRect.width)) && 
			((this.y+this.height) >= otherRect.y) && 
			(this.y <= (otherRect.y+otherRect.height))
		);
	}

	public void Log(string name)
	{
		Debug.Log(name + " x:"+x+" y:"+y+ " w:"+width+" h:"+height);
	}

	override public string ToString()
	{
		return "x:"+x+" y:"+y+ " w:"+width+" h:"+height;
	}

	public RXRect Clone()
	{
		RXRect rect = new RXRect();
		rect.x = x;
		rect.y = y;
		rect.width = width;
		rect.height = height;
		return rect;
	}
	
	public bool Contains(Vector2 point)
	{
		if(point.x < x || point.y < y || point.x > x+width || point.y > y+height)
		{
			return false;
		}
		return true;
	}

	public bool Contains(float checkX, float checkY)
	{
		if(checkX < x || checkY < y || checkX > x+width || checkY > y+height)
		{
			return false;
		}
		return true;
	}
}
