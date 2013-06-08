using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class RXDebug
{
	static RXDebug()
	{

	}

	public static void Log(params object[] objects)
	{
		StringBuilder builder = new StringBuilder();
	
		int count = objects.Length;

		for(int t = 0;t<count;t++)
		{
			builder.Append(objects[t].ToString());
			if(t < count-1) builder.Append(',');
		}

		Debug.Log(builder.ToString());
	}
}














